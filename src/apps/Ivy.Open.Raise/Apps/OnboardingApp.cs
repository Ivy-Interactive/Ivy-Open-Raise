namespace Ivy.Open.Raise.Apps;

public enum Step { Company, Team, Raise, Deck }

[App(isVisible: true, icon: Icons.Rocket, path: ["Hidden"])]
public class OnboardingApp : ViewBase
{
    object GetStep() => Step.Company switch
    {
        Step.Company => new CompanyStepView(),
        Step.Raise => new RaiseStepView(),
        Step.Deck => new DeckStepView(),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public override object? Build()
    {
        var step = UseState(Step.Company);

        return Layout.Vertical()
               | Text.H1("Welcome to Ivy Raise!")
               | GetStep();
    }
}

public record CompanyDetails()
{
    [Required]
    public string CompanyName  { get; set; }

    [Required] 
    public string CountryId { get; set; }

    public string Website { get; set; }

    [Required]
    public DateTime DateOfIncorporation { get; set; } 
}


public class CompanyStepView : ViewBase
{
    public override object? Build()
    {
        var companyDetails = UseState(new CompanyDetails());

        return companyDetails.ToForm();
    }
}

public class FoundersStepView : ViewBase
{
    public override object? Build()
    {
        return "Founders Step";
    }
}

public class RaiseStepView : ViewBase
{
    public override object? Build()
    {
        return "Raise Step";
    }
}

public class DeckStepView : ViewBase
{
    public override object? Build()
    {
        return "Deck Step";
    }
}
