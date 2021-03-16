using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Chase
    {
        public static Vector2 interceptVelocity(Vector2 projectileAcc, Vector2 targetAcc, Vector2 targetVelocity, Vector2 dist, double projectileSpeed)
        {
            // https://www.alglib.net/translator/man/manual.csharp.html#sub_polynomialsolve
            int degree = 2;
            double[] coeffs = new double[degree + 1]; // 0 index is constant, degree index is coeff of x^degree
            alglib.polynomialsolverreport unused = new alglib.polynomialsolverreport();
            alglib.complex[] roots;

            Vector2 a = targetAcc - projectileAcc;
            coeffs[0] = Vector2.Dot(dist, dist);
            coeffs[1] = 2 * Vector2.Dot(targetVelocity, targetVelocity);
            coeffs[2] = Vector2.Dot(a, dist) + Vector2.Dot(targetVelocity, targetVelocity) - Math.Pow(projectileSpeed, 2);
            //coeffs[3] = Vector2.Dot(a, targetVelocity);
            //coeffs[4] = Vector2.Dot(a, a) / 4.0;
            alglib.polynomialsolve(coeffs, degree, out roots, out unused);

            double t = 0;
            bool flag = false;

            foreach (alglib.complex root in roots)
            {
                if (root.x + root.y == root.x && root.x > 0)
                {
                    t = root.x;
                    flag = true;
                }
            }
            if (!flag)
            {
                Debug.Log("no solution");
                return new Vector2(-1000, -1000);
            }

            Vector2 travel = dist + targetVelocity * (float)t + (targetAcc / 2.0f) * (float)Math.Pow(t, 2);
            return travel / (float)t - (projectileAcc / 2.0f) * (float)t;
        }

        public static double interceptAngle(double projectileAcc, Vector2 targetVelocity, Vector2 dist)
        {
            return Math.Acos(1.0 / (-((projectileAcc*(dist.x*dist.x - 2*dist.x*dist.y + dist.y*dist.y)) / (-2*targetVelocity.x *targetVelocity.x *dist.y + 2*dist.x *targetVelocity.x *targetVelocity.y + 2*targetVelocity.x *dist.y *targetVelocity.y - 2*dist.x *targetVelocity.y *targetVelocity.y))));
        }
    }
}
