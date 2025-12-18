// Unity-Twitch-Chat Integration - Threading Support
// Original: https://github.com/lexonegit/Unity-Twitch-Chat
// Adapted for Per Aspera threading requirements

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PerAspera.Core;

namespace PerAspera.SDK.TwitchIntegration.Vendor.UnityTwitchChat
{
    public partial class TwitchConnection
    {
        private Thread? _readThread;
        private Thread? _writeThread;
        private readonly object _threadLock = new();
        private volatile bool _shouldStop;
        
        private void StartReadThread()
        {
            lock (_threadLock)
            {
                if (_readThread?.IsAlive == true)
                {
                    LogAspera.Warning("Read thread already running");
                    return;
                }
                
                _shouldStop = false;
                _readThread = new Thread(ReadThreadWorker)
                {
                    Name = "TwitchConnection-ReadThread",
                    IsBackground = true
                };
                _readThread.Start();
                
                LogAspera.Debug("Read thread started");
            }
        }
        
        private void StartWriteThread()
        {
            lock (_threadLock)
            {
                if (_writeThread?.IsAlive == true)
                {
                    LogAspera.Warning("Write thread already running");
                    return;
                }
                
                _shouldStop = false;
                _writeThread = new Thread(WriteThreadWorker)
                {
                    Name = "TwitchConnection-WriteThread",
                    IsBackground = true
                };
                _writeThread.Start();
                
                LogAspera.Debug("Write thread started");
            }
        }
        
        private void ReadThreadWorker()
        {
            try
            {
                if (_tcpClient?.GetStream() is not NetworkStream stream)
                {
                    LogAspera.Error("Cannot get NetworkStream for reading");
                    return;
                }
                
                var buffer = new byte[_readBufferSize];
                var messageBuilder = new StringBuilder();
                
                LogAspera.Debug("Read thread worker started");
                
                while (!_shouldStop && _tcpClient.Connected)
                {
                    try
                    {
                        if (stream.DataAvailable)
                        {
                            var bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                messageBuilder.Append(data);
                                
                                ProcessIncomingData(messageBuilder);
                            }
                        }
                        
                        Thread.Sleep(_readInterval);
                    }
                    catch (Exception ex) when (!_shouldStop)
                    {
                        LogAspera.Error($"Read thread error: {ex.Message}");
                        OnError?.Invoke(ex);
                        
                        // Wait before retry
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Read thread fatal error: {ex.Message}");
                OnError?.Invoke(ex);
            }
            finally
            {
                LogAspera.Debug("Read thread stopped");
            }
        }
        
        private void WriteThreadWorker()
        {
            try
            {
                if (_tcpClient?.GetStream() is not NetworkStream stream)
                {
                    LogAspera.Error("Cannot get NetworkStream for writing");
                    return;
                }
                
                LogAspera.Debug("Write thread worker started");
                
                while (!_shouldStop && _tcpClient.Connected)
                {
                    try
                    {
                        if (_writeQueue.TryDequeue(out var message))
                        {
                            var data = Encoding.UTF8.GetBytes($"{message}\r\n");
                            stream.Write(data, 0, data.Length);
                            stream.Flush();
                            
                            LogAspera.Debug($"Sent IRC: {message}");
                        }
                        
                        Thread.Sleep(_writeInterval);
                    }
                    catch (Exception ex) when (!_shouldStop)
                    {
                        LogAspera.Error($"Write thread error: {ex.Message}");
                        OnError?.Invoke(ex);
                        
                        // Wait before retry
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Write thread fatal error: {ex.Message}");
                OnError?.Invoke(ex);
            }
            finally
            {
                LogAspera.Debug("Write thread stopped");
            }
        }
        
        private void ProcessIncomingData(StringBuilder messageBuilder)
        {
            var data = messageBuilder.ToString();
            var lines = data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            // Keep partial line in buffer
            var lastLineComplete = data.EndsWith("\r\n");
            messageBuilder.Clear();
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // If this is the last line and message didn't end with \r\n, keep it in buffer
                if (i == lines.Length - 1 && !lastLineComplete)
                {
                    messageBuilder.Append(line);
                    continue;
                }
                
                ProcessIRCMessage(line);
            }
        }
        
        private void ProcessIRCMessage(string message)
        {
            try
            {
                LogAspera.Debug($"Received IRC: {message}");
                
                // Handle PING/PONG to keep connection alive
                if (message.StartsWith("PING "))
                {
                    var pongMessage = message.Replace("PING", "PONG");
                    _writeQueue.Enqueue(pongMessage);
                    return;
                }
                
                // Handle chat messages and other IRC events
                if (message.Contains("PRIVMSG"))
                {
                    var chatMessage = ParseChatMessage(message);
                    if (!string.IsNullOrEmpty(chatMessage))
                    {
                        OnMessageReceived?.Invoke(chatMessage);
                    }
                }
                
                // Handle other IRC messages (JOIN, PART, etc.)
                _alertQueue.Enqueue(message);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error processing IRC message: {ex.Message}");
                OnError?.Invoke(ex);
            }
        }
        
        private string ParseChatMessage(string ircMessage)
        {
            try
            {
                // Parse format: :username!username@username.tmi.twitch.tv PRIVMSG #channel :message
                var parts = ircMessage.Split(' ');
                if (parts.Length < 4) return string.Empty;
                
                var userPart = parts[0].Substring(1); // Remove leading ':'
                var username = userPart.Split('!')[0];
                
                var messagePart = ircMessage.Substring(ircMessage.IndexOf(':', 1) + 1);
                
                return $"{username}: {messagePart}";
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error parsing chat message: {ex.Message}");
                return string.Empty;
            }
        }
        
        private void StopThreads()
        {
            _shouldStop = true;
            
            try
            {
                _readThread?.Join(1000);
                _writeThread?.Join(1000);
            }
            catch (Exception ex)
            {
                LogAspera.Warning($"Error stopping threads: {ex.Message}");
            }
        }
    }
}