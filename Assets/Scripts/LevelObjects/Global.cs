using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace LevelObjects
{
	public static class Global
	{
		public const float ClipperPrecision = 10000f;
		public const int Layers = 5;

		public static IntPoint CalculateCentroidForOutsidePath(Paths paths)
		{
			IntPoint centroid = new IntPoint(0, 0);

			foreach (Path p in paths)
			{
				if (!Clipper.Orientation(p))
				{
					double signedArea = 0;
					float x0 = 0;
					float y0 = 0;
					float x1 = 0;
					float y1 = 0;
					float a = 0;

					for (int i = 0; i < p.Count - 1; i++)
					{
						x0 = p[i].X;
						y0 = p[i].Y;
						x1 = p[i + 1].X;
						y1 = p[i + 1].Y;
						a = x0 * y1 - x1 * y0;
						signedArea += a;
						centroid.X += (long)((x0 + x1) * a);
						centroid.Y += (long)((y0 + y1) * a);
					}

					x0 = p[p.Count - 1].X;
					y0 = p[p.Count - 1].Y;
					x1 = p[0].X;
					y1 = p[0].Y;
					a = x0 * y1 - x1 * y0;
					signedArea += a;
					centroid.X += (long)((x0 + x1) * a);
					centroid.Y += (long)((y0 + y1) * a);

					signedArea *= 0.5;
					centroid.X /= (long)(6 * signedArea);
					centroid.Y /= (long)(6 * signedArea);
				}


			}

			return centroid;
		}

		public static Path OffsetPath(Path path, IntPoint offset)
		{
			Path newPath = new Path(path.Count);
			foreach (IntPoint i in path)
			{
				long x = i.X + offset.X;
				long y = i.Y + offset.Y;
				newPath.Add(new IntPoint(x, y));
			}
			return newPath;
		}

		public static Paths OffsetPaths(Paths paths, IntPoint offset)
		{
			Paths offsetPaths = new Paths(paths.Count);
			foreach (Path p in paths)
			{
				Path newPath = OffsetPath(p, offset);
				offsetPaths.Add(newPath);
			}

			return offsetPaths;
		}
	}
}
