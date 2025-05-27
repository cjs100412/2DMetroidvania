using System.Collections;
using UnityEngine;

public interface IBossPattern
{
    //패턴을 실행할 수 있는지 여부
    bool CanExecute(BossController boss, Transform player);

    //패턴 실행 코루틴
    IEnumerator Execute(BossController boss, Transform player);

    //쿨타임 시간 계산
    float Cooldown {  get; }
}
