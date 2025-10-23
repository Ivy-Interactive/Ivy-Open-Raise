import React from "react";
import styles from "./Feature.module.scss";
import Card from "../Card/Card";
import { Zap, Shield, Rocket } from "lucide-react";

interface FeatureCardProps {
  icon: React.ReactNode;
  title: string;
  description: string;
  imageUrl?: string;
}

const FeatureCard: React.FC<FeatureCardProps> = ({
  icon,
  title,
  description,
  imageUrl,
}) => {
  return (
    <Card variant="hover" padding="large" className={styles.featureCard}>
      {imageUrl ? (
        <img src={imageUrl} alt={title} className={styles.featureImage} />
      ) : (
        <div className={styles.iconWrapper}>{icon}</div>
      )}
      <h3 className={styles.featureTitle}>{title}</h3>
      <p className={styles.featureDescription}>{description}</p>
    </Card>
  );
};

interface FeatureProps {
  features?: FeatureCardProps[];
}

const Feature: React.FC<FeatureProps> = ({ features }) => {
  const defaultFeatures: FeatureCardProps[] = [
    {
      icon: <Zap size={40} />,
      title: "Fast Fundraising",
      description:
        "Connect with investors quickly and efficiently. Our platform streamlines the entire fundraising process, from initial pitch to final closing.",
    },
    {
      icon: <Shield size={40} />,
      title: "Secure Platform",
      description:
        "Your data is protected with enterprise-grade security. We ensure confidentiality throughout your fundraising journey with encrypted communications.",
    },
    {
      icon: <Rocket size={40} />,
      title: "Scale Your Growth",
      description:
        "Access a network of top-tier VCs and accelerators. Get the funding and mentorship you need to take your startup to the next level.",
    },
  ];

  const displayFeatures = features || defaultFeatures;

  return (
    <section className={styles.feature}>
      <div className={styles.container}>
        <div className={styles.header}>
          <h2 className={styles.sectionTitle}>Why Choose Open Raise</h2>
          <p className={styles.sectionSubtitle}>
            Everything you need to raise capital successfully
          </p>
        </div>
        <div className={styles.featuresGrid}>
          {displayFeatures.map((feature, index) => (
            <FeatureCard key={index} {...feature} />
          ))}
        </div>
      </div>
    </section>
  );
};

export default Feature;