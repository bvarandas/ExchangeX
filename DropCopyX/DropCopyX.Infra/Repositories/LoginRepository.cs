using DropCopyX.Core.Entities;
using DropCopyX.Core.Repositories;
using DropCopyX.Infra.Data;
using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using Error = FluentResults.Error;
namespace DropCopyX.Infra.Repositories;
public class LoginRepository : ILoginRepository
{
    private readonly IDropCopyContext _context;
    private readonly ILogger<LoginRepository> _logger;
    private readonly MD5 _md5;
    public LoginRepository(ILogger<LoginRepository> logger, IDropCopyContext context)
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

            var insertResult = await _context.Login.BulkWriteAsync(inserts, null, cancellation);
            result = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
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
            var filter = builder.Empty;
            if (!string.IsNullOrEmpty(login.Id))
            {
                var searchFilter = builder.Regex(x => x.Id, new BsonRegularExpression(login.Id));
                filter &= searchFilter;
            }
            var loginFound = _context.Login.Find(filter).FirstOrDefault();
            if (loginFound != null)
            {
                var password = _md5.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
                var passwordFound = loginFound.Password;

                if (!loginFound.GrantedIPs.Contains(login.ActualIP))
                {
                    result.Errors.Add(new Error("IP not whitelisted."));
                    result = false;
                }

                if (passwordFound is null || !password.Equals(passwordFound))
                {
                    result.Errors.Add(new Error("password is not valid."));
                    result = false;
                }

                if (!login.Active)
                {
                    result.Errors.Add(new Error("User is not active."));
                    result = false;
                }
                
                if (result.Errors.Count.Equals(0))
                    result = true;

            }else
            {
                result.Errors.Add(new Error("Login not found"));
                result = false;
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
