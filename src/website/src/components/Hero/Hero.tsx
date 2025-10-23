import React from "react";
import styles from "./Hero.module.scss";
import Card from "../Card/Card";
import {
  SquareArrowOutUpRight,
  Play,
  Pause,
  Volume2,
  VolumeX,
} from "lucide-react";
import { Button } from "../ui/button";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";

interface HeroProps {
  title?: string;
  description?: string;
  h2Title?: string;
  smallTitle?: string;
  primaryCtaText?: string;
  secondaryCtaText?: string;
  onPrimaryCtaClick?: () => void;
  onSecondaryCtaClick?: () => void;
  mediaType?: "image" | "video";
  mediaSrc?: string;
  mediaAlt?: string;
  ctaMediaType?: "image" | "video";
  ctaMediaSrc?: string;
  ctaMediaAlt?: string;
}

// Helper function to render text with line breaks
const renderTextWithLineBreaks = (text: string) => {
  return text.split("\n").map((line, index, array) => (
    <React.Fragment key={index}>
      {line}
      {index < array.length - 1 && <br />}
    </React.Fragment>
  ));
};

const Hero: React.FC<HeroProps> = ({
  title = "Hero title",
  description = "Description",
  h2Title = "Success Stories",
  smallTitle = "Small title",
  primaryCtaText = "Join the waitlist",
  secondaryCtaText = "Book a meeting",
  onPrimaryCtaClick,
  onSecondaryCtaClick,
  mediaType,
  mediaSrc,
  mediaAlt = "Demo",
  ctaMediaType,
  ctaMediaSrc,
  ctaMediaAlt = "Success Story",
}) => {
  const [isPlaying, setIsPlaying] = React.useState(true);
  const [isMuted, setIsMuted] = React.useState(true);
  const videoRef = React.useRef<HTMLVideoElement>(null);

  const togglePlayPause = () => {
    if (videoRef.current) {
      if (isPlaying) {
        videoRef.current.pause();
      } else {
        videoRef.current.play();
      }
      setIsPlaying(!isPlaying);
    }
  };

  const toggleMute = () => {
    if (videoRef.current) {
      videoRef.current.muted = !isMuted;
      setIsMuted(!isMuted);
    }
  };
  return (
    <div className={styles.hero}>
      <div className={styles.container}>
        <header className={styles.header}>
          <img src={logoSvg} alt="Logo" className={styles.logo} />
        </header>

        <div className={styles.content}>
          <div className={styles.textContent}>
            <h1 className={styles.heroTitle}>
              {renderTextWithLineBreaks(title)}
            </h1>
            <p className={styles.description}>
              {renderTextWithLineBreaks(description)}
            </p>

            <Card variant="cta" padding="none" className={styles.ctaSection}>
              {ctaMediaSrc && (
                <div className={styles.ctaMediaContainer}>
                  {ctaMediaType === "video" ? (
                    <>
                      <video
                        ref={videoRef}
                        className={styles.ctaMedia}
                        autoPlay
                        loop
                        muted={isMuted}
                        playsInline
                      >
                        <source src={ctaMediaSrc} type="video/mp4" />
                        Your browser does not support the video tag.
                      </video>
                      <div className={styles.videoControls}>
                        <button
                          className={styles.controlButton}
                          onClick={togglePlayPause}
                          aria-label={isPlaying ? "Pause" : "Play"}
                        >
                          {isPlaying ? <Pause size={20} /> : <Play size={20} />}
                        </button>
                        <button
                          className={styles.controlButton}
                          onClick={toggleMute}
                          aria-label={isMuted ? "Unmute" : "Mute"}
                        >
                          {isMuted ? (
                            <VolumeX size={20} />
                          ) : (
                            <Volume2 size={20} />
                          )}
                        </button>
                      </div>
                    </>
                  ) : (
                    <img
                      src={ctaMediaSrc}
                      alt={ctaMediaAlt}
                      className={styles.ctaMedia}
                    />
                  )}
                </div>
              )}
              <div className={styles.ctaContent}>
                <div className={styles.h2TitleContainer}>
                  <h2 className={styles.h2Title}>
                    {renderTextWithLineBreaks(h2Title)}
                  </h2>
                  <p className={styles.smallTitle}>
                    {renderTextWithLineBreaks(smallTitle)}
                  </p>
                </div>
                <div className={styles.ctaButtons}>
                  <Button
                    variant="default"
                    size="default"
                    className={styles.primaryButton}
                    onClick={onPrimaryCtaClick}
                  >
                    <SquareArrowOutUpRight />
                    {primaryCtaText}
                  </Button>
                  <Button
                    variant="outline"
                    size="default"
                    className={styles.secondaryButton}
                    onClick={onSecondaryCtaClick}
                  >
                    {secondaryCtaText}
                  </Button>
                </div>
                <p className={styles.compatibilityText}>
                  Compatible with your database of choice
                </p>
              </div>
            </Card>
          </div>

          {mediaSrc && (
            <div className={styles.mediaContent}>
              {mediaType === "video" ? (
                <video
                  className={styles.media}
                  controls
                  autoPlay
                  loop
                  muted
                  playsInline
                >
                  <source src={mediaSrc} type="video/mp4" />
                  Your browser does not support the video tag.
                </video>
              ) : (
                <img src={mediaSrc} alt={mediaAlt} className={styles.media} />
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Hero;
