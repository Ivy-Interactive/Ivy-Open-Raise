import React, { useState } from "react";
import styles from "./ProductFeatures.module.scss";
import { LayoutDashboard, GitBranch, Users, Presentation } from "lucide-react";

interface Feature {
  id: string;
  icon: React.ReactNode;
  title: string;
  description: string;
  highlights: string[];
  image?: string;
}

const ProductFeatures: React.FC = () => {
  const features: Feature[] = [
    {
      id: "dashboard",
      icon: <LayoutDashboard size={24} />,
      title: "Dashboard",
      description:
        "Get a complete overview of your fundraising progress with real-time metrics and beautiful visualizations.",
      highlights: [
        "Track total deals, interactions, and deck views",
        "Monitor investor engagement metrics",
        "Visualize investor types and deal states",
        "Daily and weekly performance charts",
      ],
    },
    {
      id: "pipeline",
      icon: <GitBranch size={24} />,
      title: "Pipeline",
      description:
        "Manage your entire fundraising pipeline with an intuitive kanban-style board that keeps deals moving forward.",
      highlights: [
        "Drag-and-drop deal management",
        "Customizable pipeline stages",
        "Deal value tracking and forecasting",
        "Activity timeline for each deal",
      ],
    },
    {
      id: "investors",
      icon: <Users size={24} />,
      title: "Investors",
      description:
        "Build and manage relationships with your investor network. Track interactions and never miss a follow-up.",
      highlights: [
        "Comprehensive investor profiles",
        "Interaction history and notes",
        "Smart tags and filtering",
        "Contact information management",
      ],
    },
    {
      id: "decks",
      icon: <Presentation size={24} />,
      title: "Decks",
      description:
        "Share your pitch decks securely and track every view. Know exactly who's engaging with your materials.",
      highlights: [
        "Secure deck sharing with custom links",
        "Real-time view tracking",
        "Download analytics",
        "Expiring links and access controls",
      ],
    },
  ];

  const [activeFeature, setActiveFeature] = useState(features[0]);

  return (
    <section className={styles.productFeatures} id="features">
      <div className={styles.container}>
        <div className={styles.header}>
          <div className={styles.badge}>
            <span className={styles.badgeIcon}>🚀</span>
            <span className={styles.badgeText}>Product Features</span>
          </div>
          <h2 className={styles.title}>
            Everything you need to
            <br />
            manage your fundraise
          </h2>
          <p className={styles.subtitle}>
            Four powerful modules designed to streamline your fundraising
            process from first pitch to final close.
          </p>
        </div>

        <div className={styles.content}>
          <div className={styles.featureTabs}>
            {features.map((feature) => (
              <button
                key={feature.id}
                className={`${styles.featureTab} ${
                  activeFeature.id === feature.id ? styles.active : ""
                }`}
                onClick={() => setActiveFeature(feature)}
              >
                <div className={styles.tabIcon}>{feature.icon}</div>
                <span className={styles.tabTitle}>{feature.title}</span>
              </button>
            ))}
          </div>

          <div className={styles.featureContent}>
            <div className={styles.featureInfo}>
              <h3 className={styles.featureTitle}>{activeFeature.title}</h3>
              <p className={styles.featureDescription}>
                {activeFeature.description}
              </p>
              <ul className={styles.featureHighlights}>
                {activeFeature.highlights.map((highlight, index) => (
                  <li key={index} className={styles.highlight}>
                    <span className={styles.checkmark}>✓</span>
                    {highlight}
                  </li>
                ))}
              </ul>
            </div>

            <div className={styles.featureImage}>
              {activeFeature.image ? (
                <img
                  src={activeFeature.image}
                  alt={activeFeature.title}
                  className={styles.screenshot}
                />
              ) : (
                <div className={styles.imagePlaceholder}>
                  <div className={styles.placeholderIcon}>
                    {activeFeature.icon}
                  </div>
                  <span className={styles.placeholderText}>
                    {activeFeature.title} Screenshot
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default ProductFeatures;

