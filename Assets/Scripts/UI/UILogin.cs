using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UILogin : CanvasPanel
{
    [Bind("IDInput")] private TMP_InputField _idInput;
    [Bind("PasswordInput")] private TMP_InputField _passwordInput;
    [Bind("LoginButton")] private UIButton _loginButton;
    [Bind("SignInButton")] private UIButton _signInButton;
    private Vector3 _originPosition;

    protected override void Initialize()
    {
        base.Initialize();

        _passwordInput.contentType = TMP_InputField.ContentType.Password;

        _loginButton.BindEvent(OnLogin);

        _signInButton.BindEvent(OnSignIn);

        _originPosition = GetComponent<RectTransform>().position;
    }

    private async void OnLogin()
    {
        var (status, message) = await Managers.Auth.Login(_idInput.text, _passwordInput.text);

        switch (status)
        {
            case FirebaseAuthManager.AuthStatus.Success:
                // 오브젝트 자체를 원래 위치로 RectTransform DOTween 이동
                GetComponent<RectTransform>().DOMove(_originPosition, 0.5f);
                MoveToNextScene();
                break;
            case FirebaseAuthManager.AuthStatus.Canceled:
                Debug.LogError("로그인 취소됨");
                break;
            case FirebaseAuthManager.AuthStatus.Failed:
                Debug.LogError("로그인 실패");
                break;
        }
    }

    private async void OnSignIn()
    {
        var (status, message) = await Managers.Auth.Create(_idInput.text, _passwordInput.text);

        switch (status)
        {
            case FirebaseAuthManager.AuthStatus.Success:
                // 회원가입 성공 UI 띄우기
                Debug.Log("회원가입 성공");
                break;
            case FirebaseAuthManager.AuthStatus.Canceled:
                Debug.LogError("회원가입 취소됨");
                break;
            case FirebaseAuthManager.AuthStatus.Failed:
                Debug.LogError("회원가입 실패");
                break;
        }
    }
    private void MoveToNextScene()
    {
        Managers.Scene.LoadScene("SelectionScene").Forget();
    }
}
