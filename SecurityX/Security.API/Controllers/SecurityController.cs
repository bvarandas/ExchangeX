using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Application.Commands;
using Security.Application.Services;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Matching.MarketData;
using System.Runtime.CompilerServices;
using System.Threading;

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

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _securityService.Get(null!, cancellationToken);

        var listSecurities = result.Value.Values.ToList();
        return View(listSecurities);
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _securityService.Get(null!, cancellationToken);
        var listSecurities = result.Value.Values.ToList();
        return View(listSecurities);
    }

    [HttpGet]
    [Route("detail/{id:string}")]
    public async Task<IActionResult> Detail(string id, CancellationToken cancellationToken)
    {
        var securities = await _securityService.Get(new[] { id }, cancellationToken);
        if (securities == null)
            return NotFound();

        var listSecurities = securities.Value.Values.ToList();

        return View(listSecurities);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Add(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result =  await _securityService.Add(security, cancellationToken);
        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Ok(security);
    }

    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Update(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _securityService.Update(security, cancellationToken);

        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Ok(security);
    }

    [HttpDelete]
    [Route("")]
    public async Task<IActionResult> Remove(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _securityService.Delete(security, cancellationToken);
        
        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Ok(security);
    }
}