using EcommerceAdminBot.Models;
using EcommerceAdminBot.Services;
using EcommerceAdminBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class MerchandiserAttendance : ComponentDialog
    {
        private readonly StateService _stateService;
        private readonly UserRepository _userRespository;
        protected readonly IConfiguration Configuration;
        //private readonly string CheckProductDialogID = "CheckProductDlg";
        public MerchandiserAttendance(StateService stateService, UserRepository userRepository, IConfiguration configuration) : base(nameof(MerchandiserAttendance))
        {
            //_stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            _userRespository = userRepository;
            Configuration = configuration;
            _stateService = stateService;


            var waterfallSteps = new WaterfallStep[]
            {
                storeNameAsync,
                LocationStepAsync,
                MessageStepAsync,
                MessageVerificationStepAsync

            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(new TextPrompt(EmailDialogID, EmailValidation);
            //AddDialog(new NumberPrompt<int>(CheckProductDialogID, ProductExistsValidation));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> storeNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please Authenticate..."), cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("To mark your attendance kindly share the store name you have visited today."),

            }, cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To mark your attendance kindly share your location."), cancellationToken);
            //return await stepContext.NextAsync(stepContext, cancellationToken);
        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            // Save the email to user profile
            userProfile.storeName = (string)stepContext.Result;
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please Authenticate..."), cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Kindly share your live location.(currently in text format)"),

            }, cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To mark your attendance kindly share your location."), cancellationToken);
            //return await stepContext.NextAsync(stepContext, cancellationToken);
        }

       
        private async Task<DialogTurnResult> MessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            // Save the email to user profile
            userProfile.location = (string)stepContext.Result;
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please Authenticate..."), cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("If there's any message for the admin, Kindly post here.(Ex. Sorry for the delay in attendance|I have come to different store today.)"),

            }, cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To mark your attendance kindly share your location."), cancellationToken);
            //return await stepContext.NextAsync(stepContext, cancellationToken);
        }
        private async Task<DialogTurnResult> MessageVerificationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            // Save the email to user profile
            userProfile.Message = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thank You Mr.{userProfile.Name}I am sending your attendance application to the Manager for confirmation"), cancellationToken);
            
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome back {userProfile.Name}, How can I help you today?"), cancellationToken);
            // trigger the power automate flow to send email
            bool status = await _userRespository.SendEmailForlocationVerificationAsync(userProfile.storeName, userProfile.Message, userProfile.Name, userProfile.location, Configuration["PowerAutomatePOSTURLAdmin"]);
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            // verify the response status of the api call
            if (status)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Verfication Email has been sent to your manager. Thank You for your response."), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("In Case you have any issues kindly contact the admin."), cancellationToken);
                userProfile.attendance = true;
                await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Verfication Email could not be sent. Please try again later. Sorry for the inconvenience caused."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        /*private async Task<bool> ProductExistsValidation(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(promptcontext.Context, () => new UserProfile());

            string product = promptcontext.Recognized.Value;

            if (await _cosmosDBClient.CheckProductAsync(product, userProfile.ValueFinder))
            {
                await promptcontext.Context.SendActivityAsync($"The {userProfile.ValueFinder} {product} already exists. Please give different {userProfile.ValueFinder}", cancellationToken: cancellationtoken);
                return false;
            }

            return true;
        }*/

    }
}