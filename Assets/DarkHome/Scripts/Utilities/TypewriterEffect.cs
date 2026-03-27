using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;

namespace DarkHome
{
    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterEffect : MonoBehaviour
    {
        private TMP_Text _textBox;

        // Basic Typewriter Functionality
        private int _currentVisibleCharacterIndex;
        private Coroutine _typewriterCoroutine;
        private bool _readyForNewText = true;

        private WaitForSeconds _simpleDelay;
        private WaitForSeconds _interpunctuationDelay;

        [Header("Typewriter Settings")]
        [SerializeField] private float charactersPerSecond = 20;
        [SerializeField] private float interpunctuationDelay = 0.5f;

        // AUDIO SETTINGS
        [Header("Audio Settings")]
        [SerializeField] private AudioClip _typingSound;     // Kéo file tiếng gõ vào đây
        [SerializeField][Range(1, 10)] private int _audioFrequency = 2; // Cứ mấy chữ thì kêu 1 lần? (2 là vừa đẹp)
        [SerializeField][Range(0f, 1f)] private float _volume = 0.5f;
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.9f, 1.1f); // Độ méo tiếng (0.9 - 1.1 là tự nhiên nhất)


        // Skipping Functionality
        public bool CurrentlySkipping { get; private set; }
        private WaitForSeconds _skipDelay;

        [Header("Skip options")]
        [SerializeField] private bool quickSkip;
        [SerializeField][Min(1)] private int skipSpeedup = 5;


        // Event Functionality
        private WaitForSeconds _textboxFullEventDelay;
        [SerializeField][Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f; // In testing, I found 0.25 to be a good value

        public static event Action CompleteTextRevealed;
        public static event Action<char> CharacterRevealed;


        private void Awake()
        {
            _textBox = GetComponent<TMP_Text>();

            _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
            _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);

            _skipDelay = new WaitForSeconds(1 / (charactersPerSecond * skipSpeedup));
            _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);
        }

        private void OnEnable()
        {
            // TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
            InputManager.onLeftMousePressed += HandleSkip;
        }

        private void OnDisable()
        {
            // TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
            InputManager.onLeftMousePressed -= HandleSkip;
        }

        #region Skipfunctionality
        private void HandleSkip()
        {
            // Debug.Log("Left Mouse Pressed");
            if (!_readyForNewText) // Click 1: Chữ đang chạy?
            {
                // DỪNG coroutine và HIỆN HẾT CHỮ
                if (_typewriterCoroutine != null)
                {
                    StopCoroutine(_typewriterCoroutine);
                }
                _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
                _readyForNewText = true; // Đánh dấu là đã xong
                CompleteTextRevealed?.Invoke();
            }
            else // Click 2: Chữ đã hiện xong?
            {
                // GỌI câu thoại tiếp theo
                // DialogueFlowController sẽ lắng nghe sự kiện này
                EventManager.Notify(GameEvents.DiaLog.NextDialogue);
            }
        }

        // Example for how to implement it in the New Input system
        // You'd have to use a PlayerController component on this gameobject and write the function's name as On[Input Action name] for this to work.
        // In this case, my Input Action is called "RightMouseClick". But: There are a ton of ways to implement checking if a button
        // has been pressed in this system. Go watch Piti's video on the different ways of utilizing the new input system: https://www.youtube.com/watch?v=Wo6TarvTL5Q

        // private void OnRightMouseClick()
        // {
        //     if (_textBox.maxVisibleCharacters != _textBox.textInfo.characterCount - 1)
        //         Skip();
        // }
        #endregion

        // Đã bị thay thế bởi hàm Run()
        private void PrepareForNewText(Object obj)
        {
            // Debug.Log("_textBox.maxVisibleCharacters >= _textBox.textInfo.characterCount " + (_textBox.maxVisibleCharacters >= _textBox.textInfo.characterCount));
            // Debug.Log("_textBox.textInfo.characterCount " + _textBox.textInfo.characterCount);
            // Debug.Log("_textBox.maxVisibleCharacters " + _textBox.maxVisibleCharacters);
            // Debug.Log("obj != _textBox " + (obj != _textBox));
            // Debug.Log("!_readyForNewText " + (!_readyForNewText));
            if (obj != _textBox || !_readyForNewText)//|| _textBox.maxVisibleCharacters >= _textBox.maxVisibleCharacters)
                return;

            CurrentlySkipping = false;
            _readyForNewText = false;

            if (_typewriterCoroutine != null)
                StopCoroutine(_typewriterCoroutine);

            _textBox.maxVisibleCharacters = 0;
            _currentVisibleCharacterIndex = 0;

            _typewriterCoroutine = StartCoroutine(Typewriter());
        }

        // Thay thay thế cho PrepareForNewText nhưng sẽ được DialogueUI gọi chạy trực tiếp để dễ theo dõi và xử lý hơn
        public void Run(string textToType)
        {
            // Đây là code copy từ PrepareForNewText, nhưng được sửa lại
            CurrentlySkipping = false;
            _readyForNewText = false;

            if (_typewriterCoroutine != null)
                StopCoroutine(_typewriterCoroutine);

            _textBox.text = textToType; // Gán text mới vào
            _textBox.maxVisibleCharacters = 0;
            _currentVisibleCharacterIndex = 0;

            _typewriterCoroutine = StartCoroutine(Typewriter());

        }

        private IEnumerator Typewriter()
        {
            yield return new WaitForEndOfFrame();
            // Debug.Log("Coroutine Typewriter started");
            TMP_TextInfo textInfo = _textBox.textInfo;

            while (_currentVisibleCharacterIndex < textInfo.characterCount + 1)
            {
                // Debug.Log("Showing char index: " + _currentVisibleCharacterIndex);
                var lastCharacterIndex = textInfo.characterCount - 1;

                if (_currentVisibleCharacterIndex >= lastCharacterIndex)
                {
                    _textBox.maxVisibleCharacters++;
                    yield return _textboxFullEventDelay;
                    CompleteTextRevealed?.Invoke();
                    _readyForNewText = true;
                    yield break;
                }

                char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;


                // Sound Loguic
                // Chỉ kêu khi không phải khoảng trắng và đúng tần suất (Frequency)
                if (!char.IsWhiteSpace(character) && _currentVisibleCharacterIndex % _audioFrequency == 0)
                {
                    if (_typingSound != null && AudioManager.Instance != null)
                    {
                        // Random cao độ để nghe như tiếng nói "Be-be-ba-bo"
                        float randomPitch = UnityEngine.Random.Range(_pitchRange.x, _pitchRange.y);

                        // Gọi hàm mới bên AudioManager
                        AudioManager.Instance.PlaySFXPitched(_typingSound, _volume, randomPitch);
                    }
                }

                _textBox.maxVisibleCharacters++;

                if (!CurrentlySkipping &&
                    (character == '?' || character == '.' || character == ',' || character == ':' ||
                     character == ';' || character == '!' || character == '-'))
                {
                    yield return _interpunctuationDelay;
                }
                else
                {
                    yield return CurrentlySkipping ? _skipDelay : _simpleDelay;
                }

                CharacterRevealed?.Invoke(character);
                _currentVisibleCharacterIndex++;
            }
        }

        private void Skip(bool quickSkipNeeded = false)
        {
            if (CurrentlySkipping)
                return;

            CurrentlySkipping = true;

            if (!quickSkip || !quickSkipNeeded)
            {
                StartCoroutine(SkipSpeedupReset());
                return;
            }

            StopCoroutine(_typewriterCoroutine);
            _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
            _readyForNewText = true;
            CompleteTextRevealed?.Invoke();
        }

        private IEnumerator SkipSpeedupReset()
        {
            yield return new WaitUntil(() => _textBox.maxVisibleCharacters == _textBox.textInfo.characterCount - 1);
            CurrentlySkipping = false;
        }
    }


}