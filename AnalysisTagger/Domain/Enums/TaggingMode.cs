// Domain/Enums/TaggingMode.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Enums
{
    public enum TaggingMode
    {
        LiveTagging,    // tagging while video records/plays in real time
        PostTagging     // tagging on an already recorded video
    }
}
