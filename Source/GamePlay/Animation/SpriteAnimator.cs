using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.Animation
{
    /// <summary>
    /// Handles sprite animation for retro pixel art characters.
    /// Supports directional animations and frame-by-frame rendering.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        [Header("Animation Data")]
        [SerializeField] private AnimationSet animationSet;
        [SerializeField] private string defaultAnimation = "idle";
        [SerializeField] private float defaultFrameRate = 8f;

        [Header("Current State")]
        [SerializeField] private string currentAnimationName;
        [SerializeField] private int currentFrame;
        [SerializeField] private float frameTimer;
        [SerializeField] private Direction currentDirection = Direction.Down;
        [SerializeField] private bool isPlaying = true;
        [SerializeField] private bool loop = true;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color tintColor = Color.white;
        [SerializeField] private float opacity = 1f;

        [Header("Directional Sprites")]
        [SerializeField] private bool useDirectionalAnimations = true;

        // Events
        public event Action<string> OnAnimationStarted;
        public event Action<string> OnAnimationEnded;
        public event Action<string, int> OnFrameChanged;

        public string CurrentAnimation => currentAnimationName;
        public int CurrentFrame => currentFrame;
        public bool IsPlaying => isPlaying;
        public bool IsLooping => loop;

        private Dictionary<string, AnimationClip> animations;
        private AnimationClip currentClip;
        private float currentFrameRate;
        private bool isInitialized = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animations = new Dictionary<string, AnimationClip>();
        }

        private void Start()
        {
            if (animationSet != null)
            {
                LoadAnimationSet(animationSet);
            }

            if (!string.IsNullOrEmpty(defaultAnimation))
            {
                PlayAnimation(defaultAnimation);
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isPlaying || currentClip == null) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / currentFrameRate;

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            currentFrame++;

            if (currentFrame >= currentClip.frames.Count)
            {
                if (loop)
                {
                    currentFrame = 0;
                    OnAnimationEnded?.Invoke(currentAnimationName);
                }
                else
                {
                    currentFrame = currentClip.frames.Count - 1;
                    isPlaying = false;
                    OnAnimationEnded?.Invoke(currentAnimationName);
                }
            }

            UpdateSprite();
            OnFrameChanged?.Invoke(currentAnimationName, currentFrame);
        }

        private void UpdateSprite()
        {
            if (currentClip == null || currentFrame >= currentClip.frames.Count) return;

            var frame = currentClip.frames[currentFrame];
            Sprite sprite = null;

            if (useDirectionalAnimations)
            {
                // Get directional sprite
                sprite = GetDirectionalSprite(frame, currentDirection);
            }
            else
            {
                sprite = frame.sprite;
            }

            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }

            // Apply visual modifiers
            spriteRenderer.color = new Color(
                tintColor.r * tintColor.a,
                tintColor.g * tintColor.a,
                tintColor.b * tintColor.a,
                opacity
            );
        }

        private Sprite GetDirectionalSprite(AnimationFrame frame, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return frame.spriteUp ?? frame.sprite;
                case Direction.Down:
                    return frame.spriteDown ?? frame.sprite;
                case Direction.Left:
                    return frame.spriteLeft ?? frame.sprite;
                case Direction.Right:
                    return frame.spriteRight ?? frame.sprite;
                default:
                    return frame.sprite;
            }
        }

        /// <summary>
        /// Load an animation set from data.
        /// </summary>
        public void LoadAnimationSet(AnimationSet set)
        {
            if (set == null) return;

            animations.Clear();

            foreach (var clip in set.clips)
            {
                animations[clip.name] = clip;
            }

            DebugLog($"Loaded {animations.Count} animations from set");
        }

        /// <summary>
        /// Play an animation by name.
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            if (!animations.TryGetValue(animationName, out var clip))
            {
                DebugLog($"Animation not found: {animationName}");
                return;
            }

            if (animationName == currentAnimationName && isPlaying) return;

            currentAnimationName = animationName;
            currentClip = clip;
            currentFrame = 0;
            frameTimer = 0;
            currentFrameRate = clip.frameRate > 0 ? clip.frameRate : defaultFrameRate;
            isPlaying = true;
            loop = clip.loop;

            UpdateSprite();
            OnAnimationStarted?.Invoke(currentAnimationName);
        }

        /// <summary>
        /// Play an animation with a specific frame rate.
        /// </summary>
        public void PlayAnimation(string animationName, float frameRate)
        {
            PlayAnimation(animationName);
            currentFrameRate = frameRate;
        }

        /// <summary>
        /// Play an animation once (non-looping).
        /// </summary>
        public void PlayAnimationOnce(string animationName, Action onComplete = null)
        {
            PlayAnimation(animationName);
            loop = false;

            if (onComplete != null)
            {
                OnAnimationEnded += (name) =>
                {
                    if (name == animationName)
                    {
                        OnAnimationEnded -= onComplete.Method.Name.Contains("<>") ? null : null;
                        onComplete();
                    }
                };
            }
        }

        /// <summary>
        /// Stop the current animation and show a static sprite.
        /// </summary>
        public void StopAnimation()
        {
            isPlaying = false;
        }

        /// <summary>
        /// Resume the current animation.
        /// </summary>
        public void ResumeAnimation()
        {
            if (currentClip != null)
            {
                isPlaying = true;
            }
        }

        /// <summary>
        /// Set the animation direction for directional sprites.
        /// </summary>
        public void SetDirection(Direction direction)
        {
            if (currentDirection != direction)
            {
                currentDirection = direction;
                UpdateSprite();
            }
        }

        /// <summary>
        /// Set the frame rate multiplier.
        /// </summary>
        public void SetFrameRateMultiplier(float multiplier)
        {
            currentFrameRate = (currentClip?.frameRate ?? defaultFrameRate) * multiplier;
        }

        /// <summary>
        /// Set a specific frame.
        /// </summary>
        public void SetFrame(int frame)
        {
            if (currentClip == null) return;

            currentFrame = Mathf.Clamp(frame, 0, currentClip.frames.Count - 1);
            frameTimer = 0;
            UpdateSprite();
        }

        /// <summary>
        /// Go to the next frame (for frame-by-frame control).
        /// </summary>
        public void NextFrame()
        {
            if (currentClip == null) return;

            currentFrame = (currentFrame + 1) % currentClip.frames.Count;
            UpdateSprite();
        }

        /// <summary>
        /// Go to the previous frame.
        /// </summary>
        public void PreviousFrame()
        {
            if (currentClip == null) return;

            currentFrame--;
            if (currentFrame < 0)
            {
                currentFrame = currentClip.frames.Count - 1;
            }
            UpdateSprite();
        }

        /// <summary>
        /// Check if an animation exists.
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            return animations.ContainsKey(animationName);
        }

        /// <summary>
        /// Get the duration of an animation in seconds.
        /// </summary>
        public float GetAnimationDuration(string animationName)
        {
            if (!animations.TryGetValue(animationName, out var clip)) return 0;
            return clip.frames.Count / (clip.frameRate > 0 ? clip.frameRate : defaultFrameRate);
        }

        /// <summary>
        /// Set the tint color for this sprite.
        /// </summary>
        public void SetTint(Color color)
        {
            tintColor = color;
            UpdateSprite();
        }

        /// <summary>
        /// Set the opacity (0-1).
        /// </summary>
        public void SetOpacity(float value)
        {
            opacity = Mathf.Clamp01(value);
            UpdateSprite();
        }

        /// <summary>
        /// Flip the sprite horizontally.
        /// </summary>
        public void FlipSprite(bool flip)
        {
            spriteRenderer.flipX = flip;
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[SpriteAnimator] {message}");
        }
    }

    /// <summary>
    /// Represents a single animation frame.
    /// </summary>
    [Serializable]
    public class AnimationFrame
    {
        public Sprite sprite;
        public Sprite spriteUp;
        public Sprite spriteDown;
        public Sprite spriteLeft;
        public Sprite spriteRight;
        public float duration = 0.1f;
        public bool hitbox; // For attack frames
    }

    /// <summary>
    /// Represents an animation clip with multiple frames.
    /// </summary>
    [Serializable]
    public class AnimationClip
    {
        public string name;
        public string category;
        public List<AnimationFrame> frames = new();
        public float frameRate = 8f;
        public bool loop = true;
    }

    /// <summary>
    /// Container for multiple animation clips.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationSet", menuName = "ChronoCiv/Animation Set")]
    public class AnimationSet : ScriptableObject
    {
        public List<AnimationClip> clips = new();
    }
}

