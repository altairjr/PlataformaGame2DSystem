using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Get Component")]
    [SerializeField] private Camera camera_;
    [SerializeField] private GameObject warrior_;

    [Header("Config")]
    [SerializeField] private bool lockedX_ = false;
    [SerializeField] private bool lockedY_ = false;
    [SerializeField] [Range(0.1f, 10f)] private float smoothingFactor_ = 2f;

    // Get positions

    private Vector3 startPos_;
    private float travelX_ => this.camera_.transform.position.x - this.startPos_.x;
    private float travelY_ => this.camera_.transform.position.y - this.startPos_.y;

    // Parallax Factor
    private float distanceWarrior => this.startPos_.z - this.warrior_.transform.position.z;
    private float clipPlane => this.camera_.transform.position.z + (this.distanceWarrior > 0 ? this.camera_.farClipPlane : -this.camera_.nearClipPlane);
    private float parallaxFactor => Mathf.Abs(this.distanceWarrior) / this.clipPlane;

    private float newX_ => this.lockedX_ ? this.startPos_.x : this.startPos_.x + (this.travelX_ * this.parallaxFactor * smoothingFactor_);
    private float newY_ => this.lockedY_ ? this.startPos_.y : this.startPos_.y + (this.travelY_ * this.parallaxFactor * smoothingFactor_);

    private void Start()
    {
        this.startPos_ = transform.position;
    }

    private void FixedUpdate()
    {
        this.transform.position = new Vector3(this.newX_, this.newY_, this.startPos_.z);
    }
}
