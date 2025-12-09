import { useState, useEffect } from "react";

interface GitHubRelease {
  tag_name: string;
  html_url: string;
  name: string;
}

interface CachedRelease {
  data: GitHubRelease;
  timestamp: number;
}

const CACHE_KEY = "github_release_cache";
const CACHE_DURATION = 24 * 60 * 60 * 1000; // 1 day in milliseconds

export function useGitHubRelease(repo: string) {
  const [release, setRelease] = useState<GitHubRelease | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchRelease = async () => {
      // Check cache first
      const cached = localStorage.getItem(`${CACHE_KEY}_${repo}`);
      if (cached) {
        const { data, timestamp }: CachedRelease = JSON.parse(cached);
        if (Date.now() - timestamp < CACHE_DURATION) {
          setRelease(data);
          setLoading(false);
          return;
        }
      }

      try {
        const response = await fetch(
          `https://api.github.com/repos/${repo}/releases/latest`
        );
        if (response.ok) {
          const data: GitHubRelease = await response.json();
          // Cache the result
          localStorage.setItem(
            `${CACHE_KEY}_${repo}`,
            JSON.stringify({ data, timestamp: Date.now() })
          );
          setRelease(data);
        }
      } catch (error) {
        console.error("Failed to fetch GitHub release:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchRelease();
  }, [repo]);

  return { release, loading };
}
