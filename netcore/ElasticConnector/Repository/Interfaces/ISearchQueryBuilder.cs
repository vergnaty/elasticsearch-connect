using ElasticConnector.Model;

namespace ElasticConnector.Repository
{
    public interface ISearchQueryBuilder<TResult>
        where TResult : class
    {
        TResult Build(SearchQueryItem query);
    }

    public interface IBaseQueryBuild<TContainer>
    {
        TContainer Equals(string key, string value);
        TContainer DoesNotEqual(string key, string value);
        TContainer Contains(string key, string value);
        TContainer GreaterThan(string key, string value);
        TContainer LessThan(string key, string value);
        TContainer StartsWith(string key, string value);
        TContainer EndsWith(string key, string value);
        TContainer DoesNotContain(string key, string value);
    }
}
