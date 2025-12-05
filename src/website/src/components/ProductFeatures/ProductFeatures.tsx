import React, { useState } from "react";
import styles from "./ProductFeatures.module.scss";
import { LayoutDashboard, GitBranch, Users, Presentation } from "lucide-react";

import dashboardImg from "@/assets/images/dashboard.png";
import pipelineImg from "@/assets/images/pipelines.png";
import investorsImg from "@/assets/images/investors.png";
import decksImg from "@/assets/images/decks.png";

interface Feature {
  id: string;
  icon: React.ReactNode;
  title: string;
  description: string;
  highlights: string[];
  image: string;
}

const ProductFeatures: React.FC = () => {
  const features: Feature[] = [
    {
      id: "dashboard",
      icon: <LayoutDashboard size={24} />,
      title: "Dashboard",
      description:
        "Get a complete overview of your fundraising progress with real-time metrics and clear visualizations.",
      highlights: [
        "Track total deals, interactions, and deck views",
        "Monitor investor engagement metrics",
        "Visualize investor types and deal states",
        "Daily and weekly performance charts",
      ],
      image: dashboardImg,
    },
    {
      id: "pipeline",
      icon: <GitBranch size={24} />,
      title: "Pipeline",
      description:
        "Manage your entire fundraising pipeline with a simple kanban-style board that keeps deals moving forward.",
      highlights: [
        "Drag-and-drop deal management",
        "Customizable pipeline stages",
        "Deal value tracking and forecasting",
        "Activity timeline for each deal",
      ],
      image: pipelineImg,
    },
    {
      id: "investors",
      icon: <Users size={24} />,
      title: "Investors",
      description:
        "Build and manage relationships with your investor network. Track interactions and never miss a follow-up.",
      highlights: [
        "Complete investor profiles",
        "Interaction history and notes",
        "Smart tags and filtering",
        "Contact information management",
      ],
      image: investorsImg,
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
      image: decksImg,
    },
  ];

  const [activeFeature, setActiveFeature] = useState(features[0]);

  return (
    <section className={styles.productFeatures} id="product">
      <div className={styles.container}>
        <div className={styles.header}>
          <div className={styles.badge}>
            <span className={styles.badgeIcon}>🚀</span>
            <span className={styles.badgeText}>Product Features</span>
          </div>
          <h2 className={styles.title}>
            A fundraising workflow that adapts
            <br /> to how you already work.
          </h2>
          <p className={styles.subtitle}>
            Open Race gives founders a clean, flexible system for managing
            investors, conversations, and pitch engagement without restructuring
            their workflow around a SaaS tool.
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
              <img
                src={activeFeature.image}
                alt={activeFeature.title}
                className={styles.screenshot}
              />
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default ProductFeatures;
