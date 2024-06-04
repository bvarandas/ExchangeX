using FluentValidation;
using QuickFix.Fields;
using Security.Application.Commands;

namespace Security.Application.Validations;
public abstract class SecurityValidation<T> : 
    AbstractValidator<T> where T: SecurityEngineCommand
{
    protected void ValidateNew()
    {
        ValidateSymbol();
        ValidateSecurityId();
        ValidateSecuritySourceId();
        ValidateSecurityStatus();
        ValidateSecurityDescription();
        ValidateSecurityTradeVolume();
        ValidateSecurityMaxTradeVol();
        ValidateSecurityMinTradeVol();
    }

    protected void ValidateUpdate()
    {
        ValidateSymbol();
        ValidateSecurityId();
        ValidateSecuritySourceId();
        ValidateSecurityStatus();
        ValidateSecurityDescription();
        ValidateSecurityTradeVolume();
        ValidateSecurityMaxTradeVol();
        ValidateSecurityMinTradeVol();
    }

    protected void ValidateRemove()
    {
        ValidateSymbol();
        ValidateSecurityId();
        //ValidateSecuritySourceId();
        //ValidateSecurityStatus();
        //ValidateSecurityDescription();
    }

    private void ValidateSymbol()
    {
        RuleFor(o => o.SecurityEngine.Symbol)
            .NotEqual(string.Empty)
            .WithMessage("2-Unknown Security")
            .WithErrorCode("2");
    }

    private void ValidateSecurityId()
    {
        RuleFor(o => o.SecurityEngine.SecurityID)
            .NotEqual(string.Empty)
            .NotEqual("0")
            .WithMessage("2-Unknown SecurityId")
            .WithErrorCode("2");
    }

    private void ValidateSecuritySourceId()
    {
        var list = new List<string> { "4", "8" };

        RuleFor(o => o.SecurityEngine.SecurityIDSource)

            .NotEqual(string.Empty)
            .NotEqual("0")
            .WithMessage("2-The Security ID Source must be 4-ISIN NUMBER or 8 Exchange Symbol")
            .WithErrorCode("2");
    }

    private void ValidateSecurityStatus()
    {
        RuleFor(o => o.SecurityEngine.SecurityStatus)
            .NotEqual(string.Empty)
            .MinimumLength(1)
            .WithMessage("2-Unknown Security Status")
            .WithErrorCode("2");
    }

    private void ValidateSecurityDescription()
    {
        RuleFor(o => o.SecurityEngine.SecurityDesc)
            .NotEqual(string.Empty)
            .MinimumLength(10)
            .WithMessage("2-Minimun length 10 to Security description")
            .WithErrorCode("2");
    }
    private void ValidateSecurityTradeVolume()
    {
        RuleFor(o => o.SecurityEngine.TradeVolType.ToString())
            .Equal("0")
            .WithMessage("2-Number of Units must be 0 to Security TradeVolType")
            .WithErrorCode("2");
    }

    private void ValidateSecurityMinTradeVol()
    {
        RuleFor(o => o.SecurityEngine.MinTradeVol)
            .GreaterThan(0)
            .WithMessage("2-Minimum order trade must be greater than 0")
            .WithErrorCode("2");
    }

    private void ValidateSecurityMaxTradeVol()
    {
        RuleFor(o => o.SecurityEngine.MaxTradeVol)
            .LessThan(1000)
            .WithMessage("2-Maximum order trade must be Less  than 1000")
            .WithErrorCode("2");
    }

}