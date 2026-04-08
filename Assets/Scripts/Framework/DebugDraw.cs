using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugDraw
{
    public static void Box(
    Vector3 center,
    Vector3 halfExtents,
    Quaternion rotation,
    Color color,
    float duration = 0f
)
    {
        Vector3[] corners = new Vector3[8];

        Vector3 hx = Vector3.right * halfExtents.x;
        Vector3 hy = Vector3.up * halfExtents.y;
        Vector3 hz = Vector3.forward * halfExtents.z;

        corners[0] = center + hx + hy + hz;
        corners[1] = center + hx + hy - hz;
        corners[2] = center + hx - hy + hz;
        corners[3] = center + hx - hy - hz;
        corners[4] = center - hx + hy + hz;
        corners[5] = center - hx + hy - hz;
        corners[6] = center - hx - hy + hz;
        corners[7] = center - hx - hy - hz;

        for (int i = 0; i < 8; i++)
            corners[i] = rotation * (corners[i] - center) + center;

        void L(int a, int b) =>
            Debug.DrawLine(corners[a], corners[b], color, duration);

        // top
        L(0, 1); L(1, 5); L(5, 4); L(4, 0);
        // bottom
        L(2, 3); L(3, 7); L(7, 6); L(6, 2);
        // sides
        L(0, 2); L(1, 3); L(4, 6); L(5, 7);
    }

    public static void Sphere(
    Vector3 center,
    float radius,
    Quaternion rotation,
    Color color,
    int segments = 24,
    float duration = 0f
)
    {
        DrawCircle(Vector3.right, Vector3.up);     // XY plane (around Z)
        DrawCircle(Vector3.up, Vector3.forward);   // YZ plane (around X)
        DrawCircle(Vector3.forward, Vector3.right); // XZ plane (around Y)

        void DrawCircle(Vector3 axisA, Vector3 axisB)
        {
            Vector3 prev = center + rotation * (axisA * radius);

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments * Mathf.PI * 2f;
                Vector3 next =
                    center +
                    rotation * (
                        axisA * Mathf.Cos(t) * radius +
                        axisB * Mathf.Sin(t) * radius
                    );

                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }
    }
    public static void Capsule(
    Vector3 center,
    float radius,
    float height,
    Quaternion rotation,
    Color color,
    int segments = 12,
    float duration = 0f
)
    {
        height = Mathf.Max(height, radius * 2f);
        float halfCylinder = height * 0.5f - radius;

        Vector3 up = rotation * Vector3.up;
        Vector3 right = rotation * Vector3.right;
        Vector3 forward = rotation * Vector3.forward;

        Vector3 topSphere = center + up * halfCylinder;
        Vector3 bottomSphere = center - up * halfCylinder;

        // Cylinder side lines
        Debug.DrawLine(topSphere + right * radius, bottomSphere + right * radius, color, duration);
        Debug.DrawLine(topSphere - right * radius, bottomSphere - right * radius, color, duration);
        Debug.DrawLine(topSphere + forward * radius, bottomSphere + forward * radius, color, duration);
        Debug.DrawLine(topSphere - forward * radius, bottomSphere - forward * radius, color, duration);

        // Rings at cylinder ends
        DrawCircle(topSphere, right, forward);
        DrawCircle(bottomSphere, right, forward);

        // Top hemisphere arcs
        DrawHemisphereArc(topSphere, right, true);
        DrawHemisphereArc(topSphere, forward, true);
        DrawHemisphereArc(topSphere, -right, true);
        DrawHemisphereArc(topSphere, -forward, true);

        // Bottom hemisphere arcs
        DrawHemisphereArc(bottomSphere, right, false);
        DrawHemisphereArc(bottomSphere, forward, false);
        DrawHemisphereArc(bottomSphere, -right, false);
        DrawHemisphereArc(bottomSphere, -forward, false);

        // -------- helpers --------

        void DrawCircle(Vector3 c, Vector3 a, Vector3 b)
        {
            Vector3 prev = c + a * radius;

            for (int i = 1; i <= segments * 2; i++)
            {
                float t = i / (float)(segments * 2) * Mathf.PI * 2f;
                Vector3 next =
                    c +
                    a * Mathf.Cos(t) * radius +
                    b * Mathf.Sin(t) * radius;

                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }

        void DrawHemisphereArc(Vector3 sphereCenter, Vector3 side, bool top)
        {
            Vector3 prev =
                sphereCenter +
                side * radius;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments * Mathf.PI * 0.5f;

                float y = Mathf.Sin(t) * radius;
                float r = Mathf.Cos(t) * radius;

                Vector3 next =
                    sphereCenter +
                    side * r +
                    up * (top ? y : -y);

                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }
    }
    public static void Capsule(
    Vector3 point0,
    Vector3 point1,
    float radius,
    Color color,
    int segments = 12,
    float duration = 0f
)
    {
        Vector3 axis = point1 - point0;
        float length = axis.magnitude;

        if (length <= Mathf.Epsilon)
            return;

        Vector3 up = axis / length;

        // Build a stable perpendicular basis
        Vector3 right = Vector3.Cross(up, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(up, Vector3.forward);
        right.Normalize();

        Vector3 forward = Vector3.Cross(right, up);

        Vector3 topSphere = point1;
        Vector3 bottomSphere = point0;

        // Cylinder side lines
        Debug.DrawLine(topSphere + right * radius, bottomSphere + right * radius, color, duration);
        Debug.DrawLine(topSphere - right * radius, bottomSphere - right * radius, color, duration);
        Debug.DrawLine(topSphere + forward * radius, bottomSphere + forward * radius, color, duration);
        Debug.DrawLine(topSphere - forward * radius, bottomSphere - forward * radius, color, duration);

        // Rings at sphere centers
        DrawCircle(topSphere, right, forward);
        DrawCircle(bottomSphere, right, forward);

        // Top hemisphere arcs
        DrawHemisphereArc(topSphere, right, true);
        DrawHemisphereArc(topSphere, forward, true);
        DrawHemisphereArc(topSphere, -right, true);
        DrawHemisphereArc(topSphere, -forward, true);

        // Bottom hemisphere arcs
        DrawHemisphereArc(bottomSphere, right, false);
        DrawHemisphereArc(bottomSphere, forward, false);
        DrawHemisphereArc(bottomSphere, -right, false);
        DrawHemisphereArc(bottomSphere, -forward, false);

        // -------- helpers --------

        void DrawCircle(Vector3 c, Vector3 a, Vector3 b)
        {
            Vector3 prev = c + a * radius;

            for (int i = 1; i <= segments * 2; i++)
            {
                float t = i / (float)(segments * 2) * Mathf.PI * 2f;
                Vector3 next =
                    c +
                    a * Mathf.Cos(t) * radius +
                    b * Mathf.Sin(t) * radius;

                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }

        void DrawHemisphereArc(Vector3 sphereCenter, Vector3 side, bool top)
        {
            Vector3 prev = sphereCenter + side * radius;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments * Mathf.PI * 0.5f;

                float y = Mathf.Sin(t) * radius;
                float r = Mathf.Cos(t) * radius;

                Vector3 next =
                    sphereCenter +
                    side * r +
                    up * (top ? y : -y);

                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }
    }



}
