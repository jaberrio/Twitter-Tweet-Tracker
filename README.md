# Twitter Tweet Tracker on Web

## Deployment
Hosted on azure app-service: https://twitter-tweet-tracker.azurewebsites.net/

[ashxxxx2@xxxx.com]

## About the web application

Twitter Tweet Tracker takes in tweets, replies, quotes and retweet data from authenticated individual accounts and generates a emotion based personality model. This application is designed to identify the quality and emotional score of your twitter account and personality. The project also showcases the ability to use twitter and the algorithms to determine the stability and quality of the societal presence of a user in twitter. Using word analysis and key context clues, we can create a model of a twitter user who might be experiencing distress and other abnormal emotional spikes. Because of the rapid nature of twitter, it can be hard for individuals to keep track of their tweets and its contextual quality. Thus, this application can help users check themselves of their twitter accounts over large period of time.

Authentication: Use the button below to sign in to your twitter account. Please note that you will be redirected to twitter for authentication and Twitter Tweet Tracker will only use your authenticated and authorized data. Twitter Tweet Tracker does not deal with any credentials and does not store any information from the analyze.

Studies used: Predicting Personality with Social Media (Jennifer Golbeck, Cristina Robles, Karen Turner), May 2011 - Used significance data of personality traits to assign parameters and weights - Used personality data to formulate analysis algorithms

This is originally a Hackathon project presented on ShellHacks 2019 and was revised for web in 2022. About 1 million tweets were analyzed to determine a global bias for the models.


## Frameworks utilized 

- ASP .NET MVC Application (with .NET 4.7.1)
- Axios
- Chart JS
- RestSharp
- Twitter oAuth1.0 Authentication
- Twitter API v2

## Instructions

    <add key="maxTweetToUse" value="50" />
    <add key="twitterAPIKey" value="x" />
    <add key="twitterAPISecret" value="xxx" />
    <add key="twitterBearerToken" value="xxx" />

Max tweet to use references the maximum number of tweets to fetch from twitter API. upto:100



webconfig requires the following appsettings to 

## Contributors

- Jullian Berrio (ja.berrio@ufl.edu)
- Peyton Marinelli (pmarinelli@ufl.edu)
- Ahsan Rohan (ahsanrohan@ufl.edu)
- Mohammad Immam (m.immam@ufl.edu)