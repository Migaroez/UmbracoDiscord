// Credit: https://github.com/KevinJump/DoStuffWithUmbraco/tree/master/Src/DoStuff.Core/RepoPattern/Persistance

using System.Collections.Generic;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace UmbracoDiscord.Core.Repositories
{
    public interface IUmbracoRepository<TModel> where TModel : class
    {
        int Count();
        void Delete(int id);
        TModel Get(int id);
        IEnumerable<TModel> GetAll(params int[] ids);
        PagedResult<TModel> GetAllPaged(int page, int pageSize, params int[] ids);
        PagedResult<TModel> GetPaged(int page, int pageSize, Sql<ISqlContext> sql);
        TModel Save(TModel item);
    }
}
