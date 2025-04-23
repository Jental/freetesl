#nullable enable

using Assets.Enums;

namespace Assets.Models
{
    public record KeywordInstance
    {
        public Keyword Keyword { get; private set; }

        public CardInstance? SourceCardInstance { get; private set; }

        public KeywordInstance(Keyword keyword, CardInstance? sourceCardInstance)
        {
            this.Keyword = keyword;
            this.SourceCardInstance = sourceCardInstance;
        }

    }
}
