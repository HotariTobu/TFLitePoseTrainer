using System.Collections.Generic;
using System.Linq;

using K4AdotNet.BodyTracking;

using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Assets.Main.UI
{
    public class PoseLabelDictionaryFormatter : MonoBehaviour
    {
        [SerializeField] protected UnityEvent<string> _onPoseLabelDictionaryFormatted = default!;

        public void Dispatch(IReadOnlyDictionary<BodyId, string?> poseLabelDictionary)
        {
            var poseLabelLines = from pair in poseLabelDictionary
                                 select $"{pair.Key}: {pair.Value}";
            var formattedString = string.Join("\n", poseLabelLines);
            _onPoseLabelDictionaryFormatted.Invoke(formattedString);
        }
    }
}
