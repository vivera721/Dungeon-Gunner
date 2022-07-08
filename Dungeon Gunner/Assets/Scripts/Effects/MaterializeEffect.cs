using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterializeEffect : MonoBehaviour
{
    /// <summary>
    /// Materialize effect coroutine - used for the materialize special effect
    /// </summary>
    public IEnumerator MaterializeRoutine(Shader materializeShader, Color materializeColor, float materializeTime, 
        SpriteRenderer[] spriteRendererArray, Material normalMaterial)
    {
        Material materializeMaterial = new Material(materializeShader);

        materializeMaterial.SetColor("_EmissionColor", materializeColor);

        // 적 생성시 material 을 지정한 Shader material 로 변환 -- 입자가 모여 생성되는 이펙트 만들어짐
        // Set materialize material in sprite renderers
        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = materializeMaterial;
        }

        float dissolveAmount = 0f;

        // materialize enemy
        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime / materializeTime;

            materializeMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            yield return null;

        }

        // 적 생성 완료시 material 을 원래 material 로 변환 -- 원래 적 이미지
        // Set standard material in sprite renderer
        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = normalMaterial;
        }


    }
}
