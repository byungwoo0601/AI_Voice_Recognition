using Google.Cloud.Speech.V1;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition
{
	[CreateAssetMenu(fileName = "GCStreamingSpeechRecognitionConfig", menuName = "Frostweep Games/GCStreamingSpeechRecognition/Config", order = 51)]
	public class Config : ScriptableObject
	{
		public RecognitionSettings recognitionSettings;

        public bool googleCredentialLoadFromResources;

		public string googleCredentialFilePath;

		public string googleCredentialJson;

		public bool interimResults;

		public Config()
		{
            recognitionSettings = new RecognitionSettings();
            interimResults = true;
			googleCredentialLoadFromResources = true;
			googleCredentialFilePath = string.Empty;
			googleCredentialJson = string.Empty;
		}

        [System.Serializable]
		public class RecognitionSettings
        {
            [Range(1, 8)]
            public int audioChannelCount;
            public bool enableSeparateRecognitionPerChannel;
            public Enumerators.LanguageCode languageCode = Enumerators.LanguageCode.ko_KR;
            public Enumerators.LanguageCode[] alternativeLanguageCodes;
            [Range(0, 10)]
            public int maxAlternatives;
            public bool profanityFilter;
            public SpeechAdaptation adaptation;
            public SpeechContext[] speechContexts;
            public bool enableWordTimeOffsets = true;
            public bool enableWordConfidence = true;
            public bool enableAutomaticPunctuation = true;
            public bool enableSpokenPunctuation = true;
            public bool enableSpokenEmojis = true;
            public SpeakerDiarizationConfig diarizationConfig;
            public Enumerators.Model model = Enumerators.Model.@default;
            public bool useEnhanced = true;

            public RecognitionSettings()
            {
                maxAlternatives = 1;
                alternativeLanguageCodes = new Enumerators.LanguageCode[0];
                speechContexts = new SpeechContext[0];
            }
        }
	}
}