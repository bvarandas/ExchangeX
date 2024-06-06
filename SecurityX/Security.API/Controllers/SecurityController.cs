using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Application.Commands;
using Security.Application.Services;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Matching.MarketData;

namespace Security.API.Controllers;
[Route("security")]
public class SecurityController : BaseController
{
    private readonly ISecurityService _securityService = null!;

    public SecurityController(
        ISecurityService securityService,
        INotificationHandler<DomainNotification> notifications)
        :base(notifications)
    {
        _securityService = securityService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var securities = await _securityService.Get(null!, cancellationToken);
        return Ok(securities);
    }

    [HttpGet]
    [Route("detail/{id:string}")]
    public async Task<IActionResult> Detail(string id, CancellationToken cancellationToken)
    {
        var securities = await _securityService.Get(new[] { id }, cancellationToken);
        if (securities == null)
            return NotFound();

        return Ok(securities);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Add(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(security);

        var result =  await _securityService.Add(security, cancellationToken);
        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Ok(security);
    }

    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Update(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(security);
        
        var result = await _securityService.Update(security, cancellationToken);

        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Ok(security);
    }

    [HttpDelete]
    [Route("")]
    public IActionResult Remove(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(security);

        _securityService.Delete(security, cancellationToken);
        
        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Ok(security);
    }




}
