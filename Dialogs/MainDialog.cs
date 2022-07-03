// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.13.1

using AdaptiveCards;
using EcommerceAdminBot.Dialogs.Operations;
using EcommerceAdminBot.Models;
using EcommerceAdminBot.Services;
using EcommerceAdminBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;
        UserRepository _userRepository;
        private readonly string EmailVerificationCodeDialogID = "EmailVerificationCodeDlg";
        StateService _stateService;
        CosmosDBClient _cosmosDBClient;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ILogger<MainDialog> logger, IConfiguration configuration, UserRepository userRepository, StateService stateService, CosmosDBClient cosmosDBClient)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;
            _userRepository = userRepository;
            _stateService = stateService;
            _cosmosDBClient = cosmosDBClient;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(EmailVerificationCodeDialogID, EmailVerificationCodeValidation));
            AddDialog(new EmailAuthenticationDialog(_stateService, userRepository, Configuration));
            AddDialog(new MerchandiserAttendance(_stateService, userRepository, configuration));
            AddDialog(new AddProductsDialog(Configuration, cosmosDBClient, _stateService));
            AddDialog(new UpdateProductDialog(_cosmosDBClient, _stateService));
            AddDialog(new RemoveProductsDialog(_cosmosDBClient, _stateService));
            AddDialog(new ViewAllProductsDialog(_cosmosDBClient, _stateService));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                AuthenticateStepAsync,
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your name?")
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AuthenticateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            //UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                userProfile.Name = (string)stepContext.Result;

                await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            if (string.IsNullOrEmpty(userProfile.Email))
            {
                /*return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your name?")
                }, cancellationToken); */
                return await stepContext.BeginDialogAsync(nameof(EmailAuthenticationDialog), null, cancellationToken);
            }
            else
            {
                if (userProfile.UserAuthenticated)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome back {userProfile.Name}, How can I help you today?"), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello {userProfile.Name}, Your email is not verified."), cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(EmailAuthenticationDialog), null, cancellationToken);
                }

            }
            /*if (userProfile.UserAuthenticated)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome back {userProfile.Name}, How can I help you today?"), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(EmailAuthenticationDialog), null, cancellationToken);
                /*await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait, while I send an OTP to your email."), cancellationToken);
                Random rnd = new Random();
                userProfile.OTP = rnd.Next(100000, 999999);
                await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

                // trigger the power automate flow to send email
                bool status = await _userRepository.SendEmailForCodeVerificationAsync(userProfile.OTP, Configuration["PowerAutomatePOSTURL"]);

                if (status)
                {
                    return await stepContext.PromptAsync(EmailVerificationCodeDialogID, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("I have sent a verification code to your email. Please enter the code here to validate your email. Please Check your inbox or Spam folder for the email.")
                    }, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Verfication Email could not be sent. Please try again later. Sorry for the inconvenience caused."), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                
            }*/
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait while I authenticate with your Product Catalog..."), cancellationToken);

            await _cosmosDBClient.CreateDBConnection(Configuration["CosmosEndPointURI"], Configuration["CosmosPrimaryKey"], Configuration["CosmosDatabaseId"], Configuration["CosmosContainerID"], Configuration["CosmosPartitionKey"]);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("How can I help you today?"), cancellationToken);
            List<string> operationList = new List<string> { "Attendance","Add Products", "Update Product", "Remove Products","View All Products" };
            // Create card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
                cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if ("Attendance".Equals(operation))
            {
                if (userProfile.attendance)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Don't worry Mr. {userProfile.Name}, Your attendance has already been marked. For any issues kindly contact the manager"), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello {userProfile.Name}, Your email is not verified."), cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(MerchandiserAttendance), null, cancellationToken);
                }
            }
            if ("Add Products".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(AddProductsDialog), new ProductDetails(), cancellationToken);
            }
            else if ("Update Product".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(UpdateProductDialog), new ProductDetails(), cancellationToken);
            }
            else if ("Remove Products".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(RemoveProductsDialog), new ProductDetails(), cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(ViewAllProductsDialog), new ProductDetails(), cancellationToken);
            }
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private async Task<bool> EmailVerificationCodeValidation(PromptValidatorContext<int> promptcontext, CancellationToken cancellationtoken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(promptcontext.Context, () => new UserProfile());
            int verificationCode = promptcontext.Recognized.Value;

            if (verificationCode == userProfile.OTP)
            {
                userProfile.UserAuthenticated = true;
                await _stateService.UserProfileAccessor.SetAsync(promptcontext.Context, userProfile);
                return true;
            }
            await promptcontext.Context.SendActivityAsync("The verification code you entered is incorrect. Please enter the correct code.", cancellationToken: cancellationtoken);
            return false;
        }
    }
}
