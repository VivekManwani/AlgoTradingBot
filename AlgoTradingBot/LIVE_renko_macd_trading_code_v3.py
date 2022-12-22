"""
Created on Wed Oct  5 16:20:35 2022

@author: viveksitoxxx
"""
# =============================================================================
# Automated trading script I - MACD
# Author : Mayank Rasu

# Please report bug/issues in the Q&A section
# =============================================================================

import numpy as np
from stocktrends import Renko
import statsmodels.api as sm
import time
import copy
import oandapyV20
import oandapyV20.endpoints.instruments as instruments
import oandapyV20.endpoints.orders as orders
import oandapyV20.endpoints.trades as trades
import oandapyV20.endpoints.positions as positions
import pandas as pd
import warnings
from pandas.core.common import SettingWithCopyWarning
#import schedule

# Ignore warnings
warnings.simplefilter(action="ignore", category=SettingWithCopyWarning)

#initiating API connection and defining trade parameters
token_path = "token_oanda.txt"
client = oandapyV20.API(access_token=open(token_path,'r').read(),environment="practice")
account_id = "101-012-23126595-001"

#pairs = ['AAPL.us']
pairs = ['EUR_USD','GBP_USD','USD_CHF','AUD_USD','USD_CAD', 'USD_JPY', 'XAU_USD', 'BTC_USD', 'ETH_USD']
pos_size = 1000 #max capital allocated/position size for any currency pair

def candles(instrument):
    params = {"count": 201, "granularity": "M5"} #granularity can be in seconds S5 - S30, minutes M1 - M30, hours H1 - H12, days D, weeks W or months M
    candles = instruments.InstrumentsCandles(instrument=instrument,params=params)
    client.request(candles)
    ohlc_dict = candles.response["candles"]
    ohlc = pd.DataFrame(ohlc_dict)
    ohlc_df = ohlc.mid.dropna().apply(pd.Series)
    ohlc_df["volume"] = ohlc["volume"]
    ohlc_df.index = pd.to_datetime(ohlc["time"], format='%Y-%m-%dT%H:%M:%S.%fZ')
    ohlc_df = ohlc_df.apply(pd.to_numeric)
    ohlc_df.columns = ["Open","High","Low","Close","Volume"]
    return ohlc_df


def market_order(instrument, units, sl, price, signal):
    """units can be positive or negative, stop loss (in pips) added/subtracted to price """  
    #sl = max(0.0005, round(sl, 4))
    
    if instrument == 'BTC_USD':
        units = 0.01
    elif instrument == 'XAU_USD' or instrument == 'ETH_USD':
        units = 1
    
    if instrument == 'USD_JPY':
        sl = max(0.0005, round(sl, 3))
    elif instrument == 'BTC_USD':
        sl = max(0.0005, round(sl, 1))
    elif instrument == 'ETH_USD':
        sl = max(0.0005, round(sl, 2))
    else:
        sl = max(0.0005, round(sl, 4))
    
    data = {
            "order": {
            "price": "",
            "trailingStopLossOnFill": {
                "timeInForce": "GTC",
                "distance": str(sl)
                                      },
            "timeInForce": "FOK",
            "instrument": str(instrument),
            "units": str(units),
            "type": "MARKET",
            "positionFill": "DEFAULT"
                    }
            }
    r = orders.OrderCreate(accountID=account_id, data=data)
    client.request(r)


def close_positions(instrument, signal):
    if signal == 'short':
        data = {
                "shortUnits": "ALL"
                }
    elif signal == 'long':
        data = {
                "longUnits": "ALL"
                }
    r = positions.PositionClose(accountID=account_id, instrument=instrument, data=data)
    client.request(r)


def MACD(DF,a,b,c):
    """function to calculate MACD
       typical values a = 12; b =26, c =9"""
    df = DF.copy()
    df["MA_Fast"]=df["Close"].ewm(span=a,min_periods=a).mean()
    df["MA_Slow"]=df["Close"].ewm(span=b,min_periods=b).mean()
    df["MACD"]=df["MA_Fast"]-df["MA_Slow"]
    df["Signal"]=df["MACD"].ewm(span=c,min_periods=c).mean()
    df.dropna(inplace=True)
    return (df["MACD"],df["Signal"])


def ATR(DF,n):
    "function to calculate True Range and Average True Range"
    df = DF.copy()
    df['H-L']=abs(df['High']-df['Low'])
    df['H-PC']=abs(df['High']-df['Close'].shift(1))
    df['L-PC']=abs(df['Low']-df['Close'].shift(1))
    df['TR']=df[['H-L','H-PC','L-PC']].max(axis=1,skipna=False)
    df['ATR'] = df['TR'].rolling(n).mean()
    #df['ATR'] = df['TR'].ewm(span=n,adjust=False,min_periods=n).mean()
    df2 = df.drop(['H-L','H-PC','L-PC'],axis=1)
    return df2


def slope(ser,n):
    "function to calculate the slope of n consecutive points on a plot"
    slopes = [i*0 for i in range(n-1)]
    for i in range(n,len(ser)+1):
        y = ser[i-n:i]
        x = np.array(range(n))
        y_scaled = (y - y.min())/(y.max() - y.min())
        x_scaled = (x - x.min())/(x.max() - x.min())
        x_scaled = sm.add_constant(x_scaled)
        model = sm.OLS(y_scaled,x_scaled)
        results = model.fit()
        slopes.append(results.params[-1])
    slope_angle = (np.rad2deg(np.arctan(np.array(slopes))))
    return np.array(slope_angle)


def bar_check(srs):
    if abs(srs.values[-1] - srs.values[-2]) == 0:
        bar_check(srs[:-1])
    elif abs(srs.values[-1] - srs.values[-2]) <= 2:
        return True
    else:
        return False


def renko_DF(DF):
    "function to convert ohlc data into renko bricks"
    df = DF.copy()
    df.reset_index(inplace=True)
    df = df.iloc[:,[0,1,2,3,4,5]]
    df.columns = ["date","open","high","low","close","volume"]
    df2 = Renko(df)
    df2.brick_size = round(ATR(DF,120)["ATR"].values[-1],4) # added .values
    renko_df = df2.get_ohlc_data()
    renko_df["bar_num"] = np.where(renko_df["uptrend"]==True,1,np.where(renko_df["uptrend"]==False,-1,0))
    for i in range(1,len(renko_df["bar_num"])):
        if renko_df["bar_num"][i]>0 and renko_df["bar_num"][i-1]>0:
            renko_df["bar_num"][i]+=renko_df["bar_num"][i-1]
        elif renko_df["bar_num"][i]<0 and renko_df["bar_num"][i-1]<0:
            renko_df["bar_num"][i]+=renko_df["bar_num"][i-1]
    renko_df.drop_duplicates(subset="date",keep="last",inplace=True)
    return renko_df


def renko_merge(DF):
    "function to merging renko df with original ohlc df"
    df = copy.deepcopy(DF)
    df["Date"] = df.index
    renko = renko_DF(df)
    renko.columns = ["Date","open","high","low","close","uptrend","bar_num"]
    merged_df = df.merge(renko.loc[:,["Date","bar_num"]],how="outer",on="Date")
    merged_df["bar_num"].fillna(method='ffill',inplace=True)
    merged_df["macd"]= MACD(merged_df,12,26,9)[0]
    merged_df["macd_sig"]= MACD(merged_df,12,26,9)[1]
    merged_df["hist"]= merged_df["macd"] - merged_df["macd_sig"]
    merged_df["macd_slope"] = slope(merged_df["macd"],5)
    merged_df["macd_sig_slope"] = slope(merged_df["macd_sig"],5)
    merged_df["200_ma"] = np.array(df["Close"].ewm(span=200,min_periods=200).mean())
    merged_df["ATR"] = np.array(ATR(DF, 120)['ATR'])
    return merged_df


def trade_signal(MERGED_DF,l_s):
    "function to generate signal"
    signal = ""
    df = copy.deepcopy(MERGED_DF)
    if l_s == "":
        if df["bar_num"].tolist()[-1]>=2 and df["bar_num"].tolist()[-2]>0 and \
            0>df["macd"].tolist()[-1] > df["macd_sig"].tolist()[-1] and \
            df["macd_slope"].tolist()[-1] > df["macd_sig_slope"].tolist()[-1] and \
            df["Close"].tolist()[-1] > df["200_ma"].tolist()[-1] and \
            bar_check(df["bar_num"]):
            signal = "Buy"
            print('row: \n', df.iloc[-1])
        elif df["bar_num"].tolist()[-1]<=-2 and df["bar_num"].tolist()[-2]<0 and \
            0<df["macd"].tolist()[-1] < df["macd_sig"].tolist()[-1] and \
            df["macd_slope"].tolist()[-1] < df["macd_sig_slope"].tolist()[-1] and \
            df["Close"].tolist()[-1] < df["200_ma"].tolist()[-1] and \
            bar_check(df["bar_num"]):
            signal = "Sell"
            print('row: \n', df.iloc[-1])
    
    elif l_s == "long":
        if df["bar_num"].tolist()[-1] < 0:
            signal = "Close"
            print('row: \n', df.iloc[-1])
    
    elif l_s == "short":
        if df["bar_num"].tolist()[-1] > 0:
            signal = "Close"
            print('row: \n', df.iloc[-1])
    return signal


#currency = 'BTC_USD'
def main():
    #try:
    r = trades.OpenTrades(accountID=account_id)
    open_pos = client.request(r)['trades']
    open_pos_curr = {}
    if len(open_pos)>0:
        for i in range(len(open_pos)):
            open_pos_curr[open_pos[i]['instrument']] = int(open_pos[i]['currentUnits'])
    for currency in pairs:
        print('Analysing ', currency)
        long_short = ""
        if currency in open_pos_curr.keys():
            if len(open_pos_curr) > 0:
                if open_pos_curr[currency] > 0:
                    long_short = "long"
                elif open_pos_curr[currency] < 0:
                    long_short = "short"   
        ohlc = candles(currency)
        a = renko_merge(ohlc)[:-1]
        signal = trade_signal(a, long_short)
        
        if signal == "Buy":
            market_order(currency,pos_size,2*ATR(ohlc,120)['ATR'][-1], ohlc['Close'][-1], signal)
            print("New long position initiated for ", currency)
        elif signal == "Sell":
            market_order(currency,-1*pos_size,2*ATR(ohlc,120)['ATR'][-1], ohlc['Close'][-1], signal)
            print("New short position initiated for ", currency)
        elif signal == "Close":
            close_positions(currency, long_short)
            print("All positions closed for ", currency)
        #elif signal == "Close_Buy":
        #    close_positions(currency, long_short)
        #    print("Existing Short position closed for ", currency)
        #    market_order(currency,pos_size,3*ATR(ohlc,120)['ATR'][-1], ohlc['Close'][-1], signal)
        #    print("New long position initiated for ", currency)
        #elif signal == "Close_Sell":
        #    close_positions(currency, long_short)
        #    print("Existing long position closed for ", currency)
        #    market_order(currency,-1*pos_size,3*ATR(ohlc,120)['ATR'][-1], ohlc['Close'][-1], signal)
        #    print("New short position initiated for ", currency)
    #except:
    #    print("error encountered....skipping this iteration")

"""
ohlc_viz = {}
for currency in pairs:
    ohlc_viz[currency] = renko_merge(candles(currency))[:-1]


for i in range(150, len(ohlc)):
    trade_signal(ohlc_viz['AUD_USD'][:i], "")

"""
end = False 
# Continuous execution    
def job():
    global end
    starttime=time.time()
    timeout = time.time() + 60*60*5  # 60 seconds times 60 meaning the script will run for 1 hr
    while time.time() <= timeout:
        try:
            print("passthrough at ",time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(time.time())))
            main()
            print('sleeping... \n')
            time.sleep(300 - ((time.time() - starttime) % 300.0)) # 1 minute interval between each new execution
        except KeyboardInterrupt:
            print('\n\nKeyboard exception received. Exiting.')
            exit()
    end = True
    close()


# Close all positions and exit
def close():
    for currency in pairs:
        print("closing all positions for ", currency)
        try:
            close_positions(currency, 'long')
            print('Closed short positions for ', currency)
        except:
            print('No short positions opened for ', currency)
        
        try:
            close_positions(currency, 'short')
            print('Closed short positions for ', currency)
        except:
            print('No short positions opened for ', currency)


# RUN THIS AT THE END (Schedule)
#schedule.every().day.at("08:50:01").do(job)

while not end:
    if time.localtime(time.time()).tm_min % 5 == 0:
        #time.sleep(3)
        job()
    #else: 
    #    time.sleep(300 - time.localtime(time.time()).tm_sec)
    #schedule.run_pending()
    time.sleep(3)
