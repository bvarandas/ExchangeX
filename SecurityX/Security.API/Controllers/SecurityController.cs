using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Application.Services;
using SecurityX.Core.Interfaces;
using SecurityX.Core.Notifications;
using SharedX.Core.Entities;
namespace Security.API.Controllers;
[ApiController]
[Route("[controller]")]
public class SecurityController : BaseController
{
    private readonly ISecurityService _securityService = null!;
    private readonly ISecurityCache _securityCache = null!;
    public SecurityController(
        ISecurityService securityService,
        ISecurityCache securityCache,
        INotificationHandler<DomainNotification> notifications)
        :base(notifications)
    {
        _securityCache = securityCache;
        _securityService = securityService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _securityService.Get(null!, cancellationToken);

        var listSecurities = result.Value.Values.ToList();
        return Ok(listSecurities);
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _securityService.Get(null!, cancellationToken);

        if (result.Value is null)
            return NotFound();

        var listSecurities = result.Value!.Values.ToList();
        return Ok(listSecurities);
    }

    [HttpGet]
    [Route("detail/{id}")]
    public async Task<IActionResult> Detail(string id, CancellationToken cancellationToken)
    {
        var securities = await _securityService.Get(new[] { id }, cancellationToken);
        if (securities == null)
            return NotFound();

        var listSecurities = securities.Value.Values.ToList();

        return Ok(listSecurities);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Add([FromBody]SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result =  await _securityService.Add(security, _securityCache, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.Errors.First());
        
        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return Created("", security);
    }

    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Update(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _securityService.Update(security, _securityCache, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.Errors.First());

        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Registrado com sucesso!";

        return NoContent();
    }

    [HttpDelete]
    [Route("")]
    public async Task<IActionResult> Remove(SecurityEngine security, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _securityService.Delete(security, _securityCache, cancellationToken);
        
        if (result.IsFailed)
            return BadRequest(result.Errors.First());

        if (IsValidOperation())
            ViewBag.Sucesso = "Ativo Removido  com sucesso!";

        return Ok(security);
    }
}