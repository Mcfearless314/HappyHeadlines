using Core.Entites;

namespace Core.Interfaces;

public interface IArticleRepository
{
    public Task<Article> CreateArticle();
    public Task<List<Article>> GetArticles();
    public Task<Article> UpdateArticle();
    public Task DeleteArticle(int id);
}