window.initializeComponent = (name, parameters) => {
    console.log({ name: name, parameters: parameters });
}

async function initComponents() {
    const targetElement = document.getElementsByTagName('article');
    await Blazor.rootComponents.add(targetElement, 'moltenobsidian-display-remote', { });
}