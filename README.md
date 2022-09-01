<!-- # EcommerceMerchandizerBot -->
<h1 align="center">EcommerceMerchandizerBot</h1>

  <p align="center">
    Today’s customers are extremely demanding, expecting fast, inspiring and relevant shopping experiences in every moment of the customer journey. Merchandizing is the heart for retailers succeeding in delivering relevant customer experience, which is a continuous perpetual challenge, highly correlated to optimizing conversion, sales and increased revenue.
 <br>
 Go To Market bot is an Enterprise AI Chatbot, a conversation agent that manages the on field staff (e-commerce merchandiser or vendors) of multinational companies operating from different locations and retail stores.
    <br />
    <a href="https://github.com/mayank-cse/EcommerceMerchandizerBot"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://www.youtube.com/watch?v=7rRr-QrRWckhttps">View Demo</a>
    ·
    <a href="https://github.com/mayank-cse/EcommerceMerchandizerBot/issues">Report Bug</a>
    ·
    <a href="https://github.com/mayank-cse/EcommerceMerchandizerBot/files/9400239/GoToMarket.Presentation.pptxhttps://github.com/github_username/repo_name/issues">View Presentation</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#prerequisites">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#overview">Overview</a></li>
        <li><a href="#install-net-core-cli">Installation</a></li>
        <li><a href="#create-a-luis-application-to-enable-language-understanding">Enable LUIS</a></li>
      </ul>
    </li>
    <li><a href="#to-try-this-sample">Try This Sample</a></li>
    <li><a href="#testing-the-bot-using-bot-framework-emulator">Emulator Testing</a></li>
    <li><a href="#deploy-the-bot-to-azure">Deploying</a></li>
    <li><a href="#implementation">Implementation</a></li>
    <li><a href="#flow-chart">Flow Chart</a></li>
    <li><a href="#presentation">Presentation</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#further-reading">Further Reading</a></li>
  </ol>
</details>


<!-- ABOUT THE PROJECT -->
## About The Project

![App Screenshot](https://github.com/mayank-cse/EcommerceMerchandizerBot/blob/main/Assets/images/Bot%20Features%20-%20Merchandisers%20Daily%20Task.png)

This chatbot is designed for the merchandisers to :
* Mark Attendance with current GPS location update. (Thus, the admin can keep a check on their geographic coordinates)
* Submit Display Product Status with uploaded images(photographs).
* Share Market Intelligence / Information.
* Report Daily Retail Counter Sell Out.

Key Features of the Product :
* Instant Response with automatic time-to-time pop-ups (alerts).
* Assist managing team with easy to keep track functionality.
* Works as stress buster with clear and timely reporting.
* Automating Business Operations for visible efficiency gains due to fast communication.
<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Built With
Bot Framework v4 core bot sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to:

- Use [LUIS](https://www.luis.ai) to implement core AI capabilities
- Implement a multi-turn conversation using Dialogs
- Handle user interruptions for such things as `Help` or `Cancel`
- Prompt for and validate requests for information from the user
- 
<p align="right">(<a href="#ecommercemerchandizerbot">back to top</a>)</p>

## Prerequisites

This sample **requires** prerequisites in order to run.

### Overview

This bot uses [LUIS](https://www.luis.ai), an AI based cognitive service, to implement language understanding.

### Install .NET Core CLI

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- If you don't have an Azure subscription, create a [free account](https://azure.microsoft.com/free/).
- Install the latest version of the [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest) tool. Version 2.0.54 or higher.

### Create a LUIS Application to enable language understanding

The LUIS model for this example can be found under `CognitiveModels/FlightBooking.json` and the LUIS language model setup, training, and application configuration steps can be found [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-v4-luis?view=azure-bot-service-4.0&tabs=cs).

Once you created the LUIS model, update `appsettings.json` with your `LuisAppId`, `LuisAPIKey` and `LuisAPIHostName`.

```json
  "LuisAppId": "Your LUIS App Id",
  "LuisAPIKey": "Your LUIS Subscription key here",
  "LuisAPIHostName": "Your LUIS App region here (i.e: westus.api.cognitive.microsoft.com)"
```
<p align="right">(<a href="#ecommercemerchandizerbot">back to top</a>)</p>

## To try this sample

- In a terminal, navigate to `EcommerceAdminBot`

    ```bash
    # change into project folder
    cd EcommerceAdminBot
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `EcommerceAdminBot` folder
  - Select `EcommerceAdminBot.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.5.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages
- 
<p align="right">(<a href="#ecommercemerchandizerbot">back to top</a>)</p>

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.
<p align="right">(<a href="#ecommercemerchandizerbot">back to top</a>)</p>

## Implementation

https://www.youtube.com/watch?v=7rRr-QrRWck

## Flow Chart
![Flow Chart](https://github.com/mayank-cse/EcommerceMerchandizerBot/blob/main/Assets/images/Flow%20Chart.png)


## Presentation
<!-- [Presentation](https://docs.google.com/presentation/d/15G2gJ_l0Yf6BbbnAvDusuyo-6oca0zkx/edit?usp=sharing&ouid=110656000818841743215&rtpof=true&sd=true) -->
[GoToMarket Presentation.pptx](https://github.com/mayank-cse/EcommerceMerchandizerBot/files/9400239/GoToMarket.Presentation.pptx)
<p align="right">(<a href="#ecommercemerchandizerbot">back to top</a>)</p>

###
https://user-images.githubusercontent.com/72187020/188004026-d95557a7-f866-4400-856f-368c4b781093.mp4

<!-- CONTACT -->
## Contact

Mayank Gupta - [@MayankGuptaCse1](https://twitter.com/MayankGuptacse1) - mayank.guptacse1@gmail.com

Project Link: [https://github.com/mayank-cse/EcommerceMerchandizerBot](https://github.com/mayank-cse/EcommerceMerchandizerBot)

<p align="right">(<a href="#ecommercemerchandizerbot">back to top</a>)</p>


## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Dialogs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0)
- [Gathering Input Using Prompts](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=csharp)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
