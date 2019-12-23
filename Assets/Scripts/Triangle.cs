using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle {
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Vector3 this[int vertIndex] {
        get {
            switch (vertIndex) {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return c;
            }

            throw new System.ArgumentOutOfRangeException("vertIndex", "Index of a triangle vertex should be in the range 0-2");
        }
    }
}