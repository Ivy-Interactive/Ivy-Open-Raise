import React, { useState } from "react";
import styles from "./Navbar.module.scss";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";
import { Menu, X, Github } from "lucide-react";

interface NavbarProps {
  githubUrl?: string;
}

const Navbar: React.FC<NavbarProps> = ({
  githubUrl = "https://github.com/Ivy-Interactive/Ivy-Open-Raise",
}) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const navLinks = [
    { label: "Home", href: "#" },
    { label: "About", href: "#about" },
    { label: "Features", href: "#features" },
    { label: "Contact", href: "#contact" },
  ];

  const handleNavClick = (
    e: React.MouseEvent<HTMLAnchorElement>,
    href: string
  ) => {
    e.preventDefault();

    if (href === "#") {
      window.scrollTo({ top: 0, behavior: "smooth" });
    } else {
      const element = document.querySelector(href);
      if (element) {
        element.scrollIntoView({ behavior: "smooth" });
      }
    }

    setIsMenuOpen(false);
  };

  return (
    <nav className={styles.navbar}>
      <div className={styles.container}>
        <div className={styles.logoWrapper}>
          <img src={logoSvg} alt="OpenRaise" className={styles.logo} />
        </div>

        <div className={styles.navLinks}>
          {navLinks.map((link) => (
            <a
              key={link.label}
              href={link.href}
              className={styles.navLink}
              onClick={(e) => handleNavClick(e, link.href)}
            >
              {link.label}
            </a>
          ))}
        </div>

        <div className={styles.ctaGroup}>
          <a
            href={githubUrl}
            target="_blank"
            rel="noopener noreferrer"
            className={styles.githubBtn}
          >
            <Github size={18} />
          </a>
        </div>

        <button
          className={styles.menuButton}
          onClick={() => setIsMenuOpen(!isMenuOpen)}
        >
          {isMenuOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>

      {isMenuOpen && (
        <div className={styles.mobileMenu}>
          {navLinks.map((link) => (
            <a
              key={link.label}
              href={link.href}
              className={styles.mobileNavLink}
              onClick={(e) => handleNavClick(e, link.href)}
            >
              {link.label}
            </a>
          ))}
          <div className={styles.mobileCtaGroup}>
            <a
              href={githubUrl}
              target="_blank"
              rel="noopener noreferrer"
              className={styles.mobileGithubBtn}
            >
              <Github size={18} />
              GitHub
            </a>
          </div>
        </div>
      )}
    </nav>
  );
};

export default Navbar;
