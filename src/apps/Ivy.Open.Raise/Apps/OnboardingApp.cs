using Ivy.Core.Helpers;
using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps;

[App(isVisible: true, icon: Icons.Rocket, path: ["Hidden"])]
public class OnboardingApp : ViewBase
{
    private StepperItem[] GetSteps(int selectedIndex) =>
    [
        new("1", selectedIndex>0 ? Icons.Check : null, "Start"),
        new("2", selectedIndex>1 ? Icons.Check : null, "Company"),
        new("3", selectedIndex>2 ? Icons.Check : null, "Fundraise"),
        new("4", selectedIndex>3 ? Icons.Check : null, "Deck"),
        //new("5", null, "Invite Team"),
    ];
    
    private object GetStepViews(IState<int> stepperIndex) => stepperIndex.Value switch
    {
        0 => new WelcomeStepView(stepperIndex),
        1 => new CompanyStepView(stepperIndex),
        2 => new RaiseStepView(stepperIndex),
        3 => new DeckStepView(stepperIndex),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public override object? Build()
    {
        var stepperIndex = UseState(0);
        var steps = GetSteps(stepperIndex.Value);
        
        return Layout.TopCenter() |
               (Layout.Vertical().Margin(0, 20).Width(150)
                | new Stepper(OnSelect, stepperIndex.Value, steps).Width(Size.Full())
                | GetStepViews(stepperIndex)
               );
        
        ValueTask OnSelect(Event<Stepper, int> e)
        {
            stepperIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}

public class WelcomeStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H1("Welcome to Ivy Raise")
               | Text.Markdown("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.")
               | new Button("Get Started").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .HandleClick(stepperIndex.Incr);
    }
}

public record CompanyDetails
{
    public static CompanyDetails From(OrganizationSetting settings)
    {
        return new CompanyDetails
        {
            StartupName = settings.StartupName,
            CountryId = settings.CountryId,
            CurrencyId = settings.CurrencyId,
            StartupWebsite = settings.StartupWebsite,
            DateOfIncorporation = settings.StartupDateOfIncorporation,
            Cofounders = settings.Cofounders
        };
    }
    
    [Required]
    [Display(Name = "What is your company's name?")]
    public string? StartupName  { get; set; }

    [Required]
    [Display(Name = "Where are you based?")] //todo: eh what? - why is this cutofff
    public int CountryId { get; set; }
    
    [Required]
    [Display(Name = "What currency are you raising in?")]
    public string CurrencyId { get; set; }

    [Url]
    [Display(Name = "Do you have a website?")]
    public string? StartupWebsite { get; set; }
    
    [DataType(DataType.Date)]
    [Display(Name = "When did you incorporate your company?")]
    public DateTime? DateOfIncorporation { get; set; }

    [Required]
    [Display(Name = "How many co-founders do you have?")]
    public int Cofounders { get; set; } = 1;
}

public class CompanyStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var companyDetails = UseState<CompanyDetails?>();
        var saving = UseState(false);
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            var settings = await context.OrganizationSettings
                .FirstOrDefaultAsync() ?? throw new InvalidOperationException("Organization settings not found.");
            companyDetails.Set(CompanyDetails.From(settings));
            loading.Set(false);
        });

        // todo ivy: HandleSubmit instead of relying on UseEffect to save on change
        // UseEffect(() =>
        // {
        //     if (loading.Value) return;
        //     saving.Set(true);
        //
        //     //todo: Save to organisation
        //     
        //     saving.Set(false);
        //     stepperIndex.Incr();
        // }, [companyDetails]);
        
        if (loading.Value) return Text.Muted("Loading...");

        return Layout.Vertical()
               | Text.H2("Tell us about your startup")
               | companyDetails.ToForm().Large()
                   .Builder(e => e.CurrencyId, SelectCurrencyBuilder(factory))
                   .Builder(e => e.CountryId, SelectCountryBuilder(factory))
            ;
        
        //todo ivy: how can we affect the save button to include an Icon and loading state? Builder? Replace the SubmitTitle thing?
    }
}

public record RaiseDetails
{
    public static RaiseDetails From(OrganizationSetting settings)
    {
        return new RaiseDetails
        {
            RaiseTargetMin = settings.RaiseTargetMin ?? 0,
            RaiseTargetMax = settings.RaiseTargetMax ?? 0,
            RaiseTicketSize = settings.RaiseTicketSize ?? 0,
            StartupStageId = settings.StartupStageId
        };
    }
    
    [Required]
    [Display(Name = "Minimum Amount")]
    public int RaiseTargetMin { get; set; }

    [Required]
    [Display(Name = "Maximum Amount")]
    public int RaiseTargetMax { get; set; }
    
    [Required]
    [Display(Name = "Minimal Ticket Size")]
    public int RaiseTicketSize { get; set; }
    
    [Required]
    [Display(Name = "What stage are you at?")]
    public int StartupStageId { get; set; }
}

public class RaiseStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var raiseDetails = UseState<RaiseDetails?>();
        var saving = UseState(false);
        var loading = UseState(true);
        
        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            var settings = await context.OrganizationSettings
                .FirstOrDefaultAsync() ?? throw new InvalidOperationException("Organization settings not found.");
            raiseDetails.Set(RaiseDetails.From(settings));
            loading.Set(false);
        });
        
        // UseEffect(() =>
        // {
        //     saving.Set(true);
        //
        //     //todo: Save to organisation
        //     
        //     saving.Set(false);
        //     stepperIndex.Incr();
        // }, [raiseDetails]);
        
        if (loading.Value) return Text.Muted("Loading...");

        return Layout.Vertical()
               | Text.H2("How much are you raising?")
               | raiseDetails.ToForm().Large()
                   .PlaceHorizontal(e => e.RaiseTargetMin, e => e.RaiseTargetMax)
                   .Builder(e => e.StartupStageId, SelectStartupStageBuilder(factory))
            ;
    }
}

public record DeckDetails()
{
    [Required]
    public FileUpload<BlobInfo>? File { get; init; }
}

public class DeckStepView(IState<int> stepperIndex) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckDetails = UseState(new DeckDetails());
        var saving = UseState(false);
        
        UseEffect(() =>
        {
            saving.Set(true);

            //todo: Save to organisation
            
            saving.Set(false);
            stepperIndex.Incr();
        }, [deckDetails]);
        
        return Layout.Vertical()
               | Text.H2("Upload your deck")
               | deckDetails.ToForm().Large()
                   .Builder(e => e.File, FileUploadBuilder)
            ;
    }
}
