using System;
using System.Collections.Generic;
using System.Text;

namespace Gatekeeper.Models
{
    class Config
    {
        public int BaseCharReq { get; set; }
        public int BaseScore { get; set; }
        public int AdditionalCharsScore { get; set; }
        public int PromoThreshold { get; set; }
        public int RequiredWords { get; set; }

        public Config (int baseCharReq, int baseScore, int additionalCharsScore, int promoThreshold, int requiredWords)
        {
            BaseCharReq = baseCharReq;
            BaseScore = baseScore;
            AdditionalCharsScore = additionalCharsScore;
            PromoThreshold = promoThreshold;
            RequiredWords = requiredWords;
        }
    }
}
