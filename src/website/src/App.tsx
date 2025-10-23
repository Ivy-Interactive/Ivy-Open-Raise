import Hero from "./components/Hero/Hero";

function App() {
  const handleCtaClick = () => {
    // TODO: Implement search or navigation to fund discovery
    console.log("Explore funds clicked");
  };

  return (
    <div className="min-h-screen">
      <Hero
        title="Raise Capital with Confidence"
        description="Connect with top-tier venture capital firms and accelerate your startup's growth. Our platform streamlines the fundraising process from pitch to close."
        smallTitle="Join thousands of founders who've raised over $10B"
        ctaText="Explore Investors"
        onCtaClick={handleCtaClick}
      />
    </div>
  );
}

export default App;
