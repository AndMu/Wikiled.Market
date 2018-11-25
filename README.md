# Twitter Bot - Market Trend Prediction using Sentiment Analysis

Based on published [reserarch paper](https://github.com/AndMu/Market-Wisdom)

## Twitter bot [MarketPredGuy](https://twitter.com/MarketPredGuy)

### Sentiment Analysis

Monitors a set of tickers: AMD, GOOG, FB, MMM, CAT, AMZN, INTC, PM, MS, JPM, MU, TSLA

Collects sentiment from:

- From Twitter
- From Seeking alpha articles and comments (experimental and in progress)

### **Machnine learning** Technical analysis of stock using 15 technical indicators and sentiment signals.

Based on machine learning SVM (with the RBF kernel) classifier baseline.

In order to construct a decent baseline model, we made use of ten common technical indicators which led to a total of fifteen features as follows.

- **Moving Averages (MA)**. A moving average is frequently defined as a support or resistance level. Many basic trading strategies are centred around breaking support and resistance levels. In a rising market, a 50-day, 100-day or 200-day moving average may act as a support level, and in a falling market as resistance. We calculated 50-day, 100-day and 200-day moving averages and included each of them as a feature.
- **Williams %R**. This indicator was proposed by Larry Williams to detect when a stock was overbought or oversold. It tells us how the current price compares to the highest price over the past period (10 days).
- **Momentum (MOM)**. This indicator measures howthe price changed over the last N trading days. We used two momentum-based features, one-day momentum and five-day momentum.
- **Relative Strength Index (RSI)**. This is yet another indicator to find overbought and oversold stocks. It compares the magnitude of gains and losses over a specified period. We used the most common 14 days period.
- **Moving Average Convergence Divergence (MACD)**. This is one of the most effective momentum indicators, which shows the relationship between two moving averages. It generates three features: MACD, signal, and histogram values.
- **Bollinger Bands** is one of the most widely used technical indicators. It was developed and introduced in the 1980s by the famous technical trader John Bollinger. It represents two standard deviations away from a simple moving average, and can thus help price pattern recognition.
- **Commodity Channel Index (CCI)** is another a momentum indicator introduced by Donald Lambert in 1980. This indicator can help to identify a new trend or warn of extreme conditions by detecting overbought and oversold stocks. Its normal movement is in the range from -100 to +100, so going beyond this range is considered a BUY/SELL signal.
- **Average Directional Index (ADX)** is a non-directional indicator which quantifies the price trend strength using values from 0 to 100. It is useful for identifying strong price trends.
- **Triple Exponential Moving Average (TEMA)** was developed by Patrick Mulloy and first published in 1994. It serves as a trend indicator, and in contrast to moving averages it does not have the associated lag.
- **Average True Range (ATR)** is a non-directional volatility indicator developed by Wilder . The stocks and indexes with higher volatility typically have higher ATR. The features were all normalised to zero