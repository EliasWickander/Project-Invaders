using Mirror;
using UnityEngine;

namespace CustomToolkit.AdvancedTypes
{
    public class NetworkedSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        public bool m_persistentBetweenScenes;

        private static object s_instance = null;

        private static bool s_initialized = false;
        
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType(typeof(T)) as T;

                    if (s_instance == null)
                    {
                        return CreateInstance();
                    }
                }

                return (T)s_instance;
            }
        }

        private void Awake()
        {
            ResetInstanceIfFirstAwake();

            if (s_instance != null && s_instance != this)
            {
                Debug.LogWarning($"Destroying duplicate instance of Singleton<{typeof(T)}>");
                Destroy(gameObject);
            }
            else
            {
                s_instance = this;
                
                if(m_persistentBetweenScenes)
                    DontDestroyOnLoad(gameObject);   
                
                OnSingletonAwake();
            }
        }

        private void OnDestroy()
        {
            s_initialized = false;
            
            OnSingletonDestroy();
        }
        
        /// <summary>
        /// Called after Awake. Added for safety
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
            
        }
        
        /// <summary>
        /// Called after OnDestroy. Added for safety
        /// </summary>
        protected virtual void OnSingletonDestroy()
        {
            
        }

        /// <summary>
        /// Makes sure instance is always null on first awake. Needed if Domain Reloading is set to off since statics don't reset
        /// </summary>
        private void ResetInstanceIfFirstAwake()
        {
            if (s_initialized == false)
            {
                s_instance = null;
                s_initialized = true;
            }
        }

        /// <summary>
        /// Create object with singleton component on it
        /// </summary>
        /// <returns></returns>
        private static T CreateInstance()
        {
            GameObject newObject = new GameObject($"Singleton<{typeof(T)}>");
                        
            s_instance = newObject.AddComponent<T>();
                        
            return (T)s_instance;
        }
    }
}