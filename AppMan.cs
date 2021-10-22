using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Google;
using System;
using System.Threading.Tasks;
using MiniJSON;
using TensorFlow;

public class Globals
{
    public static string username;
    public static string userId;
    public static string email;

    public static bool isLoggedIn = false;
    public static bool loggedInWithGoogle = false;
    public static bool loggedInWithEmail = false;
}

public class AppMan : MonoBehaviour
{
    #region Auth
    public static string webClientId = "720959719617-f3j6nt2604ks32dnmh1nq7l9bg4n5h2p.apps.googleusercontent.com";
    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;
    public bool loadDashboard = false;
    public bool logOut = false;
    public bool signedUpSuccessfully = false;

    void Start()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();        // Don't add this when you are running the game in the editor 
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                }
                else
                {
                    Debug.Log("Could not resolve dependencies ... ");
                }
            }
        });
    }
    public void SignInWithGoogle() { OnSignIn(); }
    private void SignOutFromGoogle() { OnSignOut(); }
    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        Debug.Log("Calling sign in ... ");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
    private void OnSignOut()
    {
        Debug.Log("Calling SignOut ... ");
        GoogleSignIn.DefaultInstance.SignOut();
    }
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        loadDashboard = false;
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log("Got Unexpected Exception " + task.Exception);
                    Debug.Log("Got Unexpected Exception " + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled ... ");
            Debug.Log("Canceled ... ");
        }
        else
        {
            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
            Debug.Log("Email: " + task.Result.Email);
            loadDashboard = true;
            Globals.username = task.Result.DisplayName;
            Globals.email = task.Result.Email;
            Globals.userId = task.Result.UserId;
            Globals.isLoggedIn = true;
            Globals.loggedInWithGoogle = true;
            //Debug.Log("Google Id Token: " + task.Result.IdToken);
            //Debug.Log("Google Id Token: " + task.Result.IdToken);
            SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }
    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                {
                    Debug.Log("\nError code: " + inner.ErrorCode + " Message: " + inner.Message);
                    Debug.Log("\nError code: " + inner.ErrorCode + " Message: " + inner.Message);
                }
                else
                {
                    loadDashboard = true;
                    Globals.username = task.Result.DisplayName;
                    Globals.email = task.Result.Email;
                    Globals.userId = task.Result.UserId;
                    Globals.isLoggedIn = true;
                    Globals.loggedInWithGoogle = true;
                    Debug.Log("SignIn Successful ... ");
                }
            }
        });
    }
    bool EmailLogin(string username, string password)
    {
        bool res = false;
        if (username == "" || password == "")
        {
            // Empty fields 
            Debug.Log("Empty Field");
            Debug.Log("Empty Field");
            res = false;
        }
        else
        {
            FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(username, password).ContinueWith((task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    Debug.Log((AuthError)e.ErrorCode);
                    Debug.Log(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    Debug.Log((AuthError)e.ErrorCode);
                    Debug.Log(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsCompleted)
                {
                    Globals.username = username;
                    Globals.email = username;
                    Globals.userId = task.Result.UserId;
                    Globals.isLoggedIn = true;
                    Globals.loggedInWithEmail = true;
                    loadDashboard = true;
                    Debug.Log("Welcome back " + Globals.email);
                    res = true;
                }
            }));
        }
        return res;
    }
    bool RegisterUser(string email, string password)
    {
        bool res = false;
        if (password == "" || email == "")
        {
            // Empty field 
            res = false;
            Debug.Log("Some fields are empty ... ");
        }
        else
        {
            FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith((task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    Debug.Log((AuthError)e.ErrorCode);
                    Debug.Log(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    Debug.Log((AuthError)e.ErrorCode);
                    Debug.Log(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsCompleted)
                {
                    Debug.Log("Signed Up Successfully ... ");
                    signedUpSuccessfully = true;
                    Debug.Log("Signed Up Successfully ... ");
                    //HideAllPanels();
                    //HelpersMan.ShowPanel(loginPanel);
                    res = true;
                }
            }));
        }
        return res;
    }
    void LogOut()
    {
        if (Globals.loggedInWithEmail)
        {
            Globals.isLoggedIn = false;
            Globals.loggedInWithEmail = false;
            Globals.username = "";
            Globals.email = "";
            Globals.userId = "";
            loadDashboard = false;
            FirebaseAuth.DefaultInstance.SignOut();
        }
        else if (Globals.loggedInWithGoogle)
        {
            Globals.isLoggedIn = false;
            Globals.loggedInWithGoogle = false;
            Globals.username = "";
            Globals.email = "";
            Globals.userId = "";
            loadDashboard = false;
            GoogleSignIn.DefaultInstance.SignOut();
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("Signed out from google ... ");
        }
        logOut = true;
    }
    #endregion
    public GameObject loginPanel;
    public GameObject signUpPanel;
    public GameObject mainMenuPanel;

    public InputField usernameIF;
    public InputField passwordIF;
    public InputField signUpEmailIF;
    public InputField signUpPassIF;
    public InputField signUpPassConfirmIF;

    #region BtnCallbacks
    public void LoginBtnCallback()
    {
        if (usernameIF && passwordIF)
        {
            string username = usernameIF.text;
            string pass = passwordIF.text;

            Debug.Log("Username = " + username + " Password = " + pass);

            // Load Scene in single load mode 
            EmailLogin(username, pass);
        }
    }
    public void LogOutBtnCallback()
    {
        Debug.Log("Good bye come back soon ... ");

        // Load login menu 
        LogOut();
    }
    public void SignUpBtnCallback()
    {
        if (signUpEmailIF && signUpPassIF && signUpPassConfirmIF)
        {
            string username = signUpEmailIF.text;
            string pass = signUpPassIF.text;
            string passConfirm = signUpPassConfirmIF.text;

            if (pass != passConfirm)
            {
                Debug.Log("Passwords don't match ... ");
                return;
            }
            else if (username == "" || pass == "" || passConfirm == "")
            {
                Debug.Log("Some fields are empty ... ");
                return;
            }
            else
            {
                RegisterUser(username, pass);
            }
        }
    }
    public void CreateAccountBtnCallback()
    {
        ShowPanel(signUpPanel);
    }
    public void BackToLoginPanelBtnCallback()
    {
        HidePanel(signUpPanel);
    }
    #endregion

    #region UIArea
    void HidePanel(GameObject panel)
    {
        if (panel)
        {
            panel.SetActive(false);
        }
    }
    void ShowPanel(GameObject panel)
    {
        if (panel)
        {
            panel.SetActive(true);
        }
    }
    void HideAllPanels()
    {
        HidePanel(loginPanel);
        HidePanel(signUpPanel);
        HidePanel(mainMenuPanel);
    }
    #endregion

    #region Main
    void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "LoginMenu")
        {
            HideAllPanels();
            ShowPanel(loginPanel);
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            HideAllPanels();
            ShowPanel(mainMenuPanel);
            StartCoroutine(DetectLocation());

            // Assign Btns callbacks 
            if (detectObjBtn)
            {
                detectObjBtn.onClick.AddListener(delegate
                {
                    DetectObjectBtnCallback();
                });
            }
            if (classifyObjBtn)
            {
                classifyObjBtn.onClick.AddListener(delegate
                {
                    ClassifyObjectBtnCallback();
                });
            }
        }

        if (objectDetectionModel && labels)
        {
            objectDetector = new Detector(objectDetectionModel, labels, input: "image_tensor");
            objectClassifier = new Classifier(objectDetectionModel, labels);
        }
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "LoginMenu")
        {
            // Check if the user has logged in 
            if (loadDashboard && (Globals.loggedInWithEmail || Globals.loggedInWithGoogle))
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
            if (signedUpSuccessfully)
            {
                HidePanel(signUpPanel);
                signedUpSuccessfully = false;
            }
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            // Check if the user is really logged 
            //if (!(loadDashboard))
            //{
            //    SceneManager.LoadScene("LoginMenu", LoadSceneMode.Single);
            //}
            if (logOut)
            {
                SceneManager.LoadScene("LoginMenu", LoadSceneMode.Single);
                logOut = false;
            }
        }
    }
    #endregion

    #region GPSFunctionality
    public string GoogleAPIKey;
    string countryLocation;

    IEnumerator DetectLocation()
    {
        // Check if the user enabled location service 
        if (!(Input.location.isEnabledByUser))
        {
            Debug.Log("Location service is not enabled ... ");
            yield break;
        }

        // Start the service itself 
        Input.location.Start();

        // Wait until the service is initialized 
        int c = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && c > 1)
        {
            yield return new WaitForSeconds(1);
            c--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Could not detect location ... ");
            yield break;
        }
        else
        {
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude);
            Debug.Log("Accuracy: " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.verticalAccuracy);
            
        }

        //Sends the coordinates to Google Maps API to request information
        using (WWW www = new WWW("https://maps.googleapis.com/maps/api/geocode/json?latlng=" + Input.location.lastData.latitude + "," + Input.location.lastData.longitude + "&key=" + GoogleAPIKey))
        {
            yield return www;

            //if request was successfully
            if (www.error == null)
            {
                //Deserialize the JSON file
                var location = Json.Deserialize(www.text) as Dictionary<string, object>;
                var locationList = location["results"] as List<object>;
                var locationList2 = locationList[0] as Dictionary<string, object>;

                //Extract the substring information at the end of the locationList2 string
                countryLocation = locationList2["formatted_address"].ToString().Substring(locationList2["formatted_address"].ToString().LastIndexOf(",") + 2);

                //This will return the country information
                Debug.LogAssertion(countryLocation);
            }
        };

        // Stop the service 
        Input.location.Stop();

        yield break;
    }
    #endregion

    #region ObjectDetection
    public Text logTxt;

    public RawImage testImg;
    public Button detectObjBtn;
    public Button classifyObjBtn;

    [SerializeField]
    TextAsset objectDetectionModel;

    [SerializeField]
    TextAsset labels;

    Detector objectDetector;
    Classifier objectClassifier;

    void DetectObjectInImage(Texture imgTexture)
    {
        if (imgTexture && objectDetector)
        {
            Debug.Log("Detecting ... ");
            var outputs = objectDetector.Detect((Texture2D)imgTexture, angle: 90, threshold: 0.6f);
            Debug.Log(outputs);
            if (logTxt)
            {
                logTxt.text = outputs.ToString();
            }
        }
    }
    void ClassifyObjectInImage(Texture imgTexture)
    {
        if (imgTexture && objectClassifier != null)
        {
            Debug.Log("Classifing ... ");
            var outputs = objectClassifier.Classify((Texture2D)imgTexture);
            Debug.Log(outputs);
            if (logTxt)
            {
                logTxt.text = outputs.ToString();
            }
        }
    }

    void DetectObjectBtnCallback()
    {
        if (testImg)
        {
            DetectObjectInImage(testImg.mainTexture);
        }
    }
    void ClassifyObjectBtnCallback()
    {
        if (testImg)
        {
            ClassifyObjectInImage(testImg.mainTexture);
        }
    }
    #endregion
}
