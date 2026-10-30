using UnityEngine;
using Tayx.Graphy;

namespace ORBTRUDER
{
    public class OpenDebug : MonoBehaviour
    {
        [SerializeField] GraphyManager graphyManager;
        [SerializeField] private KeyCode toggleKey = KeyCode.O;

        private void Awake()
        {
            if (graphyManager == null) graphyManager = FindFirstObjectByType<GraphyManager>();
        }

        private void Start()
        {
            if (graphyManager != null)
                graphyManager.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                if (graphyManager != null)
                    ChangeGraphyVisualization();
        }

        void ChangeGraphyVisualization()
        {
            if (graphyManager.gameObject.activeSelf) graphyManager.gameObject.SetActive(false);
            else graphyManager.gameObject.SetActive(true);
        }
    }
}