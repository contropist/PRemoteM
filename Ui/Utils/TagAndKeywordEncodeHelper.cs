﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PRM.Model;
using PRM.Model.Protocol;
using PRM.Model.Protocol.Base;
using PRM.Service;
using VariableKeywordMatcher.Model;

namespace PRM.Utils
{
    public static class TagAndKeywordEncodeHelper
    {
        public static string RectifyTagName(string name)
        {
            return name.Replace("#", "").Replace(" ", "_").Trim();
        }
        public static Tuple<List<TagFilter>, List<string>> DecodeKeyword(string keyword)
        {
            var words = keyword.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var keyWords = new List<string>(words.Count);
            var tagFilters = new List<TagFilter>();
            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];
                if (word.StartsWith("#") || word.StartsWith("-#") || word.StartsWith("+#"))
                {
                    bool isExcluded = word.StartsWith("-#");
                    string tagName = word.Substring(word.IndexOf("#", StringComparison.Ordinal) + 1);
                    if (IoC.Get<GlobalData>().TagList.Any(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        tagFilters.Add(TagFilter.Create(tagName, isExcluded ? TagFilter.FilterType.Excluded : TagFilter.FilterType.Included));
                    }
                    // 考虑带空格的 tag 情况，遍历后续所有的 word，组装后查询是否是 tag
                    for (int j = i + 1; j < words.Count; j++)
                    {
                        if (words[j].StartsWith("#") || words[j].StartsWith("-#") || words[j].StartsWith("+#"))
                            break;
                        tagName = $"{tagName} {words[j]}";
                        if (IoC.Get<GlobalData>().TagList.Any(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            words[j] = "";
                            tagFilters.Add(TagFilter.Create(tagName, isExcluded ? TagFilter.FilterType.Excluded : TagFilter.FilterType.Included));
                        }
                    }
                }
                else if (string.IsNullOrEmpty(word) == false)
                {
                    keyWords.Add(word);
                }
            }

            return new Tuple<List<TagFilter>, List<string>>(tagFilters, keyWords);
        }
        public static string EncodeKeyword(List<TagFilter>? tagFilters = null, List<string>? keyWords = null)
        {
            StringBuilder sb = new StringBuilder();
            if (tagFilters != null)
                foreach (var tag in tagFilters.Distinct())
                {
                    if (tag.IsExcluded)
                        sb.Append(" -#" + tag.TagName);
                    else
                        sb.Append(" #" + tag.TagName);
                }

            if (keyWords != null)
                foreach (var keyWord in keyWords.Distinct())
                {
                    sb.Append(" " + keyWord);
                }

            var filterString = sb.ToString().Trim();
            if (string.IsNullOrWhiteSpace(filterString))
                return "";
            return filterString + (keyWords?.Count > 0 ? "" : " ");
        }
        public static Tuple<bool, MatchResults?> MatchKeywords(ProtocolBase server, IEnumerable<TagFilter>? tagFilters, IEnumerable<string>? keywords)
        {
            var kws = keywords?.ToArray();
            if (tagFilters?.Any() != true && kws?.Any() != true)
            {
                return new Tuple<bool, MatchResults?>(true, null);
            }

            // check tags
            if(tagFilters != null)
            {
                bool bTagMatched = true;
                foreach (var tagFilter in tagFilters)
                {
                    if (tagFilter.IsIncluded != server.Tags.Any(x => String.Equals(x, tagFilter.TagName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        bTagMatched = false;
                        break;
                    }
                }

                if (bTagMatched == false)
                {
                    return new Tuple<bool, MatchResults?>(false, null);
                }
            }

            // no keyword
            if (kws?.Any() != true)
            {
                return new Tuple<bool, MatchResults?>(true, null);
            }

            // match keywords
            var dispName = server.DisplayName;
            var subTitle = server.SubTitle;
            var mrs = IoC.Get<KeywordMatchService>().Match(new List<string>() { dispName, subTitle }, kws);
            if (mrs.IsMatchAllKeywords)
                return new Tuple<bool, MatchResults?>(true, mrs);

            return new Tuple<bool, MatchResults?>(false, null);
        }
    }
}
