using System;

namespace ElasticConnector.Model
{
    public class CustomFilter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime ValueDate { get; set; }
        public Conditions Condition { get; set; }
        public Operations Operation { get; set; }
    }

    public enum Operations : short
    {
        Equals,
        DoesNotEqual,
        Contains,
        DoesNotContain,
        GreaterThan,
        LessThan,
        StartsWith,
        EndsWith
    }

    public enum Conditions : short
    {
        AND,
        OR
    }
}
