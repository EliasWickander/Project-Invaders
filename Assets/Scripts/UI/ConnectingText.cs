using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ConnectingText : MonoBehaviour
{
    public TMP_Text m_connectingText;

    private void OnEnable()
    {
        OnConnectionAttempted();
    }

    public void OnConnectionAttempted()
    {
        m_connectingText.text = "Connecting...";
    }

    public void OnConnectionSuccess()
    {
        m_connectingText.text = "Connected!";
    }

    public void OnConnectionDisconnected()
    {
        m_connectingText.text = "Couldn't connect to server: Connection closed";
    }
    public void OnClientError(TransportError error)
    {
        switch (error)
        {
            case TransportError.DnsResolve:
            {
                m_connectingText.text = "Failed to resolve host. Please make sure the ip is correct";
                break;
            }
            case TransportError.Refused:
            {
                m_connectingText.text = "Server refused your connection";
                break;
            }
            case TransportError.Timeout:
            {
                m_connectingText.text = "Took too long to connect. Please make sure your internet connection is stable";
                break;
            }
            case TransportError.Congestion:
            {
                m_connectingText.text = "Disconnecting connection because it can't process data fast enough";
                break;
            }
            case TransportError.InvalidReceive:
            {
                m_connectingText.text = "Possible allocation attack. Disconnecting connection";
                break;
            }
            case TransportError.InvalidSend:
            {
                m_connectingText.text = "Sending more network data than what's allowed. Disconnecting connection";
                break;
            }
            case TransportError.ConnectionClosed:
            {
                m_connectingText.text = "Server has closed connection";
                break;
            }
            default:
            {
                m_connectingText.text = "Unknown connection error";
                break;
            }
        }
    }
}