﻿namespace Gat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class VariableBucket
    {
        internal readonly KeyValuePair<string, string>?[] Variables;

        public VariableBucket()
           : this(new KeyValuePair<string, string>?[5])
        {
        }

        public VariableBucket(KeyValuePair<string, string>?[] variables)
        {
            Variables = variables;
        }

        public void Set(int position, string key, string value)
        {
            if (position < 1 || position > 5)
                throw new ArgumentOutOfRangeException(nameof(position), "Index out of range. Use 1-5 indexes");

            Variables[position - 1] = new KeyValuePair<string, string>(key, value);
        }

        public VariableBucket MergeWith(VariableBucket other)
        {
            var variables = new KeyValuePair<string, string>?[5];

            Variables.CopyTo(variables, 0);

            for (int i = 0; i < 5; i++)
            {
                if (other.Variables[i] != null)
                {
                    variables[i] = other.Variables[i];
                }
            }

            return new VariableBucket(variables);
        }

        public bool Any()
        {
            return Variables.Any(v => v != null);
        }

        public string ToUtme()
        {
            return "8(" +
                   string.Join("*", Variables.Where(v => v != null).Select(kvp => AnalyticsClient.EncodeUtmePart(kvp.Value.Key)).ToArray()) +
                   ")9(" +
                   string.Join("*", Variables.Where(v => v != null).Select(kvp => AnalyticsClient.EncodeUtmePart(kvp.Value.Value)).ToArray()) +
                   ")11(" +
                   string.Join("*", Variables.Where(v => v != null).Select(kvp => "1").ToArray());
        }

        public void Clear(int position)
        {
            Variables[position - 1] = null;
        }
    }
}
