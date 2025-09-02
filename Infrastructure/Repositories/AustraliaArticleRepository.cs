using Core.Entites;
using Core.Interfaces;

namespace Infrastructure.Repositories;

public class AustraliaArticleRepository : IArticleRepository
{
    public Task<Article> CreateArticle()
    {
        throw new NotImplementedException();
    }

    public Task<List<Article>> GetArticles()
    {
        throw new NotImplementedException();
    }

    public Task<Article> UpdateArticle()
    {
        throw new NotImplementedException();
    }

    public Task DeleteArticle(int id)
    {
        throw new NotImplementedException();
    }
}