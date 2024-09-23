using System;
using System.Collections;
using BoilerplateRomi;
using UnityEngine;
using UnityEngine.Networking;

namespace McKenna.Communications
{
    public class ConnectionVerifier : CoreScript
    {
        private bool _success;

        public bool Success => _success;
        
        public override IEnumerator Setup(ApplicationController appController)
        {
            yield return CheckConnection();
            yield return base.Setup(appController);
        }

        private IEnumerator CheckConnection()
        {
            using (var request = UnityWebRequest.Get("https://config.aukiverse.com/aukiverse.json"))
            {
                yield return request.SendWebRequest();
                Extensions.Log($"Connection verified, status: {request.result}");
                _success = request.isDone && request.result == UnityWebRequest.Result.Success;
            }
            yield return null;
        }
    }
}
