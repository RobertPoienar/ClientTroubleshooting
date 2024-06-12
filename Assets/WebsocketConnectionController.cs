using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebsocketConnectionController : MonoBehaviour
{
    InputField hostInputField;
    InputField portInputField;
    RuntimeWebSocketClient runtimeWebSocketClient;
    // Start is called before the first frame update
    void Start()
    {
        hostInputField = GameObject.Find("Host").GetComponent<InputField>();
        portInputField = GameObject.Find("Port").GetComponent<InputField>();

        GameObject.Find("Connect").GetComponent<Button>().onClick.AddListener(OnConnect);
        GameObject.Find("Disconnect").GetComponent<Button>().onClick.AddListener(OnDisconnect);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnConnect()
    {
        var host = hostInputField.text;
        var port = int.Parse(portInputField.text);
        runtimeWebSocketClient = new RuntimeWebSocketClient(host, port);
        runtimeWebSocketClient.Connect();
    }
    public void OnDisconnect()
    {
        runtimeWebSocketClient.Stop();
    }
}
