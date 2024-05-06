using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using QuickFix.Fields;
using SharedX.Core.Entities;
using SharedX.Core.Repositories;
using Sharex.Infra.LoginFix.Data;
using System.Security.Cryptography;
using System.Text;
using Error = FluentResults.Error;
namespace ShareX.Infra.Repositories;
public class LoginFixRepository : ILoginRepository
{
    private readonly ILoginFixContext _context;
    private readonly ILogger<LoginFixRepository> _logger;
    private readonly MD5 _md5;
    public LoginFixRepository(ILogger<LoginFixRepository> logger, ILoginFixContext context)
    {
        _logger = logger;
        _context = context;
        _md5 = MD5.Create();
    }

    public async Task<Result<bool>> AddLogin(Login login, CancellationToken cancellation)
    {
        Result<bool> result = false;
        try
        {
            var inserts = new List<WriteModel<Login>>();
            var password =  _md5.ComputeHash(Encoding.UTF8.GetBytes(login.Password)); 

            login.Password = GetPasswordByte(password);

            inserts.Add(new InsertOneModel<Login>(login));

            var insertResult = _context.Login.BulkWriteAsync(inserts, null, cancellation);
            result = insertResult.Result.IsAcknowledged && insertResult.Result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    public async Task<Result<bool>> ExecuteLogin(Login login, CancellationToken cancellation)
    {
        Result<bool> result = false;
        try
        {
            var builder = Builders<Login>.Filter;
            var passwordBytes = _md5.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
            var password = GetPasswordByte(passwordBytes);

            var filter = builder.Eq(u => u.UserName, login.UserName) | 
                         builder.Eq(p=>p.Password, login.Password); 
            
            var loginFound = _context.Login.Find(filter).FirstOrDefault();
            if (loginFound != null)
            {
                var passwordFound = loginFound.Password;

                //if (!loginFound.GrantedIPs.Contains(login.ActualIP))
                //{
                //    result = Result.Fail(new Error("IP not whitelisted."));
                //}

                if (passwordFound is null || !password.Equals(passwordFound))
                {
                    result = Result.Fail(new Error("password is not valid."));
                }

                if (!login.Active)
                {
                    
                    result = Result.Fail(new Error("User is not active."));
                }
                
                if (result.Errors.Count.Equals(0))
                {
                    result = Result.Ok(true);
                }
                    

            }else
            {
                result = Result.Fail("Login not found");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }

        return result;
    }

    public async Task<Result<bool>> RemoveLogin(Login login, CancellationToken cancellation)
    {
        Result<bool> result = false;
        try
        {
            var updates = new List<WriteModel<Login>>();
            var filterBuilder = Builders<Login>.Filter;

            var filter = filterBuilder.Where(x => x.Id == login.Id);
            updates.Add(new ReplaceOneModel<Login>(filter, login));

            var updateResult = await _context.Login.BulkWriteAsync(updates, null, cancellation);
            result = updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
        catch (ArgumentNullException ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    public async Task<Result<bool>> UpdateLogin(Login login, CancellationToken cancellation)
    {
        Result<bool> result = false;
        try
        {
            var updates = new List<WriteModel<Login>>();
            var filterBuilder = Builders<Login>.Filter;

            var filter = filterBuilder.Where(x => x.Id == login.Id);
            updates.Add(new ReplaceOneModel<Login>(filter, login));

            var updateResult = await _context.Login.BulkWriteAsync(updates, null, cancellation);
            result = updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
        catch (ArgumentNullException ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    private string GetPasswordByte(byte[] data)
    {
        StringBuilder sBuilder = new StringBuilder();

        // Loop para formatar cada byte como uma String em hexadecimal
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }
}
