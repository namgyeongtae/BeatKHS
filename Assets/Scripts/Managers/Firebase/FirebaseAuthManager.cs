using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase;

public class FirebaseAuthManager : Manager
{
    private FirebaseAuth _auth;     // 로그인  / 회원가입 등에 사용
    private FirebaseUser _user;     // 인증이 완료된 유저 정보

    public enum AuthStatus
    {
        Success,
        Canceled,
        Failed
    }

    public override void Init()
    {
        _auth = FirebaseAuth.DefaultInstance; 
    }

    public async Task<(AuthStatus status, string message)> Create(string email, string password)
    {
        try
        {
            AuthResult authResult = await _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            _user = authResult.User;
            Debug.LogFormat("회원가입 성공 : {0} ({1})", _user.Email, _user.UserId);
            return (AuthStatus.Success, "회원가입에 성공했습니다.");
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("회원가입 취소됨");
            return (AuthStatus.Canceled, "회원가입이 취소되었습니다.");
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"회원가입 실패: {ex.Message}");
            return (AuthStatus.Failed, ex.Message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"회원가입 실패: {ex.Message}");
            return (AuthStatus.Failed, ex.Message);
        }
    }

    public async Task<(AuthStatus status, string message)> Login(string email, string password)
    {
        try
        {
            AuthResult authResult = await _auth.SignInWithEmailAndPasswordAsync(email, password);
            _user = authResult.User;
            Debug.LogFormat("로그인 성공 : {0} ({1})", _user.Email, _user.UserId);
            return (AuthStatus.Success, "로그인에 성공했습니다.");
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("로그인 취소됨");
            return (AuthStatus.Canceled, "로그인이 취소되었습니다.");
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"로그인 실패: {ex.Message}");
            return (AuthStatus.Failed, ex.Message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"로그인 실패: {ex.Message}");
            return (AuthStatus.Failed, ex.Message);
        }
    }

    public void Logout()
    {
        _auth.SignOut();
        Debug.Log("로그아웃 완료");
    }
}