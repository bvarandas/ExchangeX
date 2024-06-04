using Microsoft.AspNetCore.Mvc;
using SharedX.Core.Bus;
namespace Security.API.Controllers;
[Route("security")]
public class SecurityController : Controller
{
    private readonly IMediatorHandler _mediator;
    public SecurityController( IMediatorHandler mediator)
    {
        _mediator = mediator;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Route("new")]
    public IActionResult Create(SharedX.Core.Matching.MarketData.Security security)
    {
        if (!ModelState.IsValid) return View(security);

        _mediator.SendCommand(new );
        ; ; _advAppService.Register(advViewModel);

        //if (IsValidOperation())
        //    ViewBag.Sucesso = "Anúncio Registrado com sucesso!";

        return View(security);
    }

    [HttpPut]
    [Route("")]
    public IActionResult Update(SharedX.Core.Matching.MarketData.Security security)
    {
        if (!ModelState.IsValid) return View(advViewModel);
        _advAppService.Register(advViewModel);

        if (IsValidOperation())
            ViewBag.Sucesso = "Anúncio Registrado com sucesso!";

        return View(advViewModel);
    }

    pu



}
